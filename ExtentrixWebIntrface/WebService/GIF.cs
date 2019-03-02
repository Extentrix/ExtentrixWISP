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
/// Summary description for GIF
/// </summary>
public class Gif
{
	private byte [] imageData ;
    private int width;
    private int height;
    private int numColourBits;
    private int transparentColour;
    private BinaryWriter stream;
    private FileStream fStream;
    public Gif( byte[]imageData ,int imageSize, int width , int height, int  numColourBits,string fileName)
	{
		this.imageData = new byte[imageSize];
        imageData.CopyTo(this.imageData,0);
		//memcpy(this.image , image, imageSize);
		this.width = width;
		this.height = height;
		this.numColourBits = numColourBits;
		this.transparentColour = 16;
        fStream = new FileStream(Path.GetTempPath() + "/" + fileName, FileMode.OpenOrCreate, FileAccess.Write);
        //fStream = new FileStream("c:\\xx.gif", FileMode.OpenOrCreate, FileAccess.Write);
		this.stream = new BinaryWriter(fStream);//CreateFile(filePath, GENERIC_WRITE, 0 ,NULL, CREATE_ALWAYS,FILE_ATTRIBUTE_NORMAL, NULL);
		
	}

    /*
    write the image to the binary stream as a GIF
    */
    public void write()
    {
       this.addHeader();
       this.addScreenDescriptor();
       this.addVgaColourTable();
       this.addVgaColourTable(); // extra 16 entry = transparent + 15 spare
       this.addGraphicControlExtension();
       this.addImageDescriptor();
       this.addImage();
       this.addTrailer();
       this.fStream.Close();
       this.stream.Close();
    }
    // write the GIF header
	void addHeader()
	{
		
		char [] header = new char[7];
		char [] gifArray = "GIF89a".ToCharArray();
       	this.stream.Write(gifArray);
	}

//write screen descriptor
	void addScreenDescriptor()
	{
		const byte  GlobalColourBit = 0x80;
		const int  ColourDepthFieldStart = 4;
        byte settings;
		
		this.writeShort(this.width );
		this.writeShort( this.height );
    
		/* create packed field info
       Global Colour Table = 1 bit
       Colour Resolution = 3 bits
       Sort Flag = 1 bit
       Size of global Colour table = 3 bits
		*/

		settings = GlobalColourBit;
		/*
		this is usually numColourBits - 1, but we have need transparency so
		assume a one bit greater colour space
		*/
		settings |= (byte)(this.numColourBits << ColourDepthFieldStart);
		// size of colour table
		settings |= 4; // 32 entries = 2^5 store as 5-1 = 4
    

		this.write8( settings );
		this.write8( (byte)this.transparentColour ); //background colour
		this.write8(0); // aspect ratio   
	}

	// extra 16 entry = transparent + 15 spare
	void addVgaColourTable()
	{
		byte[,] vgaColours= {
			{    0,    0,    0 },
			{ 0x80,    0,    0 },
			{    0, 0x80,    0 },
			{ 0x80, 0x80,    0 },
			{    0,    0, 0x80 },
			{ 0x80,    0, 0x80 },
			{    0, 0x80, 0x80 },
			{ 0x80, 0x80, 0x80 },
			{ 0xC0, 0xC0, 0xC0 },
			{ 0xFF,    0,    0 },
			{    0, 0xFF,    0 },
			{ 0xFF, 0xFF,    0 },
			{    0,    0, 0xFF },
			{ 0xFF,    0, 0xFF },
			{    0, 0xFF, 0xFF },
			{ 0xFF, 0xFF, 0xFF }
		     };

		for ( int i = 0; i < 16; i++ )
		{
			for ( int j = 0; j < 3; j++ )
			{
				this.write8( vgaColours[i,j] );
			}
	 }
	}

	void addGraphicControlExtension()
	{
		this.write8( 0x21 );  // Extension Introducer
		this.write8( 0xf9 );  // Graphic Control Label 
		this.write8( 0x4 );   // Size
		this.write8( 0x1 );   // Flags - transparent on
		this.writeShort( 0x0 );
		this.write8( (byte)this.transparentColour );
		this.write8( 0x0 );   // Block Terminator
	}

	void addImageDescriptor()
	{
		 byte settings;
		 this.write8( 0x2c ); // Image Separator
         this.writeShort( 0 );
         this.writeShort( 0 );
		 this.writeShort( this.width );
		 this.writeShort( this.height );

		//build settings field
		settings = 0;
    
		/*
		no local colour table
		no interlace
		no sort
		reverved zero
		no local colourtable size
		*/
		this.write8( settings );
	}
	/*
	add image data
	*/
	void addImage()
	{
		/*
			LZW code size
			set it to one more than the colour bits so we can have
			transparent bit
		*/

		this.write8( (byte)(this.numColourBits + 1) );
    
		GifStream gifStream = new GifStream(this.stream);

		int  clearCode = 1 << ( this.numColourBits + 1 );
    
		int curPos = 0;
		int numPixels = this.width * this.height;
		int pixelPos = numPixels >> 3;
        int numTokens = 1 << this.numColourBits;
		gifStream.write( clearCode );
		while( curPos < numPixels)
		{
			if ( ( curPos % numTokens ) == 0 )
					gifStream.write( clearCode );
			if ( this.isMask( curPos ))
				gifStream.write( this.transparentColour );
			else
				gifStream.write( this.getPixel( curPos ) );

			curPos++;
		}

		gifStream.write( clearCode + 1 );
		gifStream.flush();
		this.write8( 0 );
	}
	
	void addTrailer()
	{
		this.write8( 0x3b ); // terminator
	}
/*private:
	char * image;
	int width;
	int height;
	int numColourBits;
	int transparentColour;
	HANDLE hFile;*/

	// write 2 bytes
	void writeShort(int s)
	{
        
        this.write8( (byte) (s & 0xFF) );
        this.write8((byte)((s >> 8) & 0xFF)); 
		 
	}
	// write 1 byte 
	void write8(byte Byte )
	{
        this.stream.Write(Byte);
	}

	/*
	Return if the current Pixel is a mask pixel
	*/
	bool isMask (int index )
	{
		int pixelIndex = index >> 3;
		byte bitPosition =(byte)( 0x1 << ( 7 - ( index & 0x7 ) ));

		return ( ( this.imageData[ pixelIndex ] & bitPosition ) != 0 );
	}

	/*
	return pixel value at position
	*/
    byte getPixel(int pos )
	{
		int numPixels = this.width * this.height;
		int pixelPos = numPixels >> 3;
		byte pixel = 0;

		if ( this.numColourBits == 4 )
		{
			int test = pixelPos + ( pos >> 1 );
            try
            {
                pixel = (byte)this.imageData[pixelPos + (pos >> 1)];
                if ((pos & 0x1) == 0) pixel >>= 4;

                pixel &= (0xF);
            }
            catch(Exception ex)
            {
                throw new Exception((pixelPos + (pos >> 1)) + " ***" + ex.Message + " ****");
            }
		}

		return pixel;
	}

	
}
