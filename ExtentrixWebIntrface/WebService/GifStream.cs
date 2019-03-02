using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

/// <summary>
/// Summary description for GifStream
/// </summary>
public class GifStream
{
	private BinaryWriter stream ;
    private int numTokenBits;
    private byte[] buffer;
    private int bufferIndex;
    private int curOutputBit ;
    private byte outputByte;

    public GifStream(BinaryWriter stream)
	{
	    this.stream = stream;
		this.numTokenBits = 6;
		this.buffer = new byte[ 256 ];
		this.bufferIndex = 1;
		this.curOutputBit = 0;
		this.outputByte = 0;
	}
    public void write(int token)
    {
        int curTokenBit = 0;
        do
        {
            byte bitMask;
            int numOutputBitsAvail = 8 - this.curOutputBit;
            int numTokenBitsNeeded = this.numTokenBits - curTokenBit;

            if (curTokenBit > this.curOutputBit)
                bitMask =(byte) ((token >> (curTokenBit - this.curOutputBit)) & 0xFF);
            else
                bitMask = (byte)((token << (this.curOutputBit - curTokenBit)) & 0xFF);

            this.outputByte |= bitMask;

            this.curOutputBit += numTokenBitsNeeded;
            curTokenBit += numOutputBitsAvail;

            if (this.curOutputBit >= 8)
            {
                this.curOutputBit = 0;
                this.buffer[this.bufferIndex] = (byte)(this.outputByte & 0xFF);
                this.bufferIndex++;

                if (this.bufferIndex == 256)
                {
                    this.flush();
                }
                this.outputByte = 0;
            }
        } while (curTokenBit <= this.numTokenBits);
    }
    public void flush()
    {
        if (this.bufferIndex > 1)
        {
            //DWORD dwWritten;
            this.buffer[0] = (byte)(this.bufferIndex - 1);
            this.stream.Write(this.buffer, 0, this.bufferIndex);//(this.buffer,0,this.bufferIndex);//!WriteFile(this.hFile, this.buffer, this.bufferIndex, &dwWritten, NULL))
           //     exit(4);
            this.bufferIndex = 1;
        }
    }
}
