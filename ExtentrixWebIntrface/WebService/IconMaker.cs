using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#region BitmapCompression
public enum IconImageFormat : int
{
    BMP = 0,
    PNG = 5,
    UNKNOWN = 255
}
#endregion

#region ICONDIR
[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct ICONDIR
{
    public UInt16 idReserved;
    public UInt16 idType;
    public UInt16 idCount;

    #region Constructors
    public ICONDIR(UInt16 reserved, UInt16 type, UInt16 count)
    {
        idReserved = reserved;
        idType = type;
        idCount = count;
    }
    #endregion

    #region Properties
    public static ICONDIR Initalizated
    {
        get { return new ICONDIR(0, 1, 1); }
    }
    #endregion

    #region Methods
    public void Write(Stream stream)
    {
        byte[] array = new byte[Marshal.SizeOf(this)];
        fixed (ICONDIR* ptr = &this)
            Marshal.Copy((IntPtr)ptr, array, 0, sizeof(ICONDIR));
        stream.Write(array, 0, sizeof(ICONDIR));
    }
    #endregion
}
#endregion

#region ICONDIRENTRY
[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct ICONDIRENTRY
{
    public byte bWidth;
    public byte bHeight;
    public byte bColorCount;
    public byte bReserved;
    public ushort wPlanes;
    public ushort wBitCount;
    public uint dwBytesInRes;
    public uint dwImageOffset;

    #region Methods
    public void Write(Stream stream)
    {
        byte[] array = new byte[Marshal.SizeOf(this)];
        fixed (ICONDIRENTRY* ptr = &this)
            Marshal.Copy((IntPtr)ptr, array, 0, sizeof(ICONDIRENTRY));
        stream.Write(array, 0, sizeof(ICONDIRENTRY));
    }
    #endregion
}
#endregion

#region BITMAPINFOHEADER
[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct BITMAPINFOHEADER
{
    public UInt32 biSize;
    public UInt32 biWidth;
    public UInt32 biHeight;
    public UInt16 biPlanes;
    public UInt16 biBitCount;
    public IconImageFormat biCompression;
    public UInt32 biSizeImage;
    public Int32 biXPelsPerMeter;
    public Int32 biYPelsPerMeter;
    public UInt32 biClrUsed;
    public UInt32 biClrImportant;

    #region Methods
    public void Write(Stream stream)
    {
        byte[] array = new byte[Marshal.SizeOf(this.GetType())];
        fixed (BITMAPINFOHEADER* ptr = &this)
            Marshal.Copy((IntPtr)ptr, array, 0, sizeof(BITMAPINFOHEADER));
        stream.Write(array, 0, sizeof(BITMAPINFOHEADER));
    }
    #endregion
}
#endregion

#region IconMaker
/// <summary>
/// Summary description for IconMaker
/// </summary>
public class IconMaker
{
    public static byte[] palette_16 ={ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x80, 0x00, 0x00, 0x80, 0x80, 0x80, 0x00, 0xc0, 0xc0, 0xc0, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00, 0x00, 0xff, 0xff, 0xff, 0x00 };
    //public static byte[] palette_256 ={ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x80, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x80, 0x00, 0x00, 0x80, 0x80, 0x80, 0x00, 0xc0, 0xc0, 0xc0, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0xff, 0x00, 0x00, 0xff, 0xff, 0xff, 0x00 };

    static public byte[] decode(string data)
    {
        byte[] imageData;    // icon image data

        char charValue_A = 'A';
        int dataSize = data.Length;

        // convert icon data to a byte array
        imageData = new byte[dataSize / 2];

        // decode encoded image data into image data
        for (int i = 0; i < dataSize / 2; i++)
        {
            imageData[i] = (byte)(((data[i * 2] - charValue_A) << 4)
                         + (data[i * 2 + 1] - charValue_A));
            //imageData[i]= data[i] - charValue_A;
        }
        return imageData;
    }

    static public string makeIcon(string imagedata)
    {
        byte[] imageBytes = decode(imagedata);
        Random random = new Random();
        string randomName = random.Next().ToString();
        Gif gif = new Gif(imageBytes, imageBytes.Length, 32, 32, 4, randomName + ".gif");
        gif.write();
        //string x = Path.GetTempPath() + randomName + ".gif";
        Bitmap image = new Bitmap(Path.GetTempPath() + randomName + ".gif");
        MemoryStream mStream = new MemoryStream();
        image.Save(mStream, ImageFormat.Gif);
        byte[] bytes = mStream.GetBuffer();
        mStream.Close();
        mStream.Dispose();
        image.Dispose();
        return Convert.ToBase64String(bytes);
        //return x;
    }

    internal static string makeIconEx(string icon, int size, int depth, string icoFormat)
    {
        // Header of the whole file, the file may contain more than one Icon

        byte[] bytes = System.Convert.FromBase64String(icon);
        int colorsInPalette = depth > 8 ? 0 : 1 << depth;

        ICONDIR iconDir             = ICONDIR.Initalizated;
        ICONDIRENTRY iconEntry      = new ICONDIRENTRY();
        BITMAPINFOHEADER bitMapHeader = new BITMAPINFOHEADER();

        iconEntry.bWidth            = (byte) size;
        iconEntry.bHeight           = (byte) size;
        iconEntry.bColorCount = (byte)colorsInPalette; // 4 = 16 colors, 8 = 256
        iconEntry.wBitCount         = (ushort) depth;
        iconEntry.bReserved         = 0;
        iconEntry.dwBytesInRes = (uint)(bytes.Length + Marshal.SizeOf(bitMapHeader) + colorsInPalette * 4); // Each color requires 4 bytes in the palette
        iconEntry.dwImageOffset     = (uint)(Marshal.SizeOf(iconDir) + Marshal.SizeOf(iconEntry));
        iconEntry.wPlanes           = 1;

        bitMapHeader.biSize = (uint)Marshal.SizeOf(bitMapHeader);
        bitMapHeader.biWidth        = (uint) size;
        bitMapHeader.biHeight       = (uint) size * 2;
        bitMapHeader.biBitCount     = (ushort) depth;
        bitMapHeader.biPlanes       = 1;
        bitMapHeader.biCompression  = IconImageFormat.BMP;
        bitMapHeader.biXPelsPerMeter = 0;
        bitMapHeader.biYPelsPerMeter = 0;
        bitMapHeader.biClrUsed = (uint) colorsInPalette;
        bitMapHeader.biClrImportant = 0;
        bitMapHeader.biSizeImage = (uint) bytes.Length;  

        //
        // The icon is recieved flipped vertically, we need first to flip it before sending
        // it to the consumer
        //
        byte[] data = null;
        int len = 0, revIndex = 0, rowWidth = 0, ANDmapSize;

        len = bytes.Length;
        data = new byte[bytes.Length];
        rowWidth = size * depth / 8;
        ANDmapSize = len - size * size * depth / 8;
        int step = 4;
        for (int k = len - rowWidth; k >= ANDmapSize; k -= rowWidth)
            for (int r = 0; r < rowWidth; r++, revIndex++)
                data[revIndex] = bytes[k + r];

        for (int l = 0, m = ANDmapSize - step; l < ANDmapSize; l += step, m -= step)
            for (int n = 0; n < step; n++, revIndex++)
                data[revIndex] = bytes[m + n];

        MemoryStream mStream = new MemoryStream();

        //
        // Write the Icon directory
        //
        iconDir.Write(mStream);

        //
        // Write the icon directory entry
        //
        iconEntry.Write(mStream);

        //
        // Write the bitmap header
        //
        bitMapHeader.Write(mStream);

        //
        // Write the appropriate palette if required
        //
        if (depth == 4)
            mStream.Write(palette_16, 0, palette_16.Length);

        //
        // Write the icon data recieved from the CPS server
        //
        mStream.Write(data, 0, data.Length);

        byte[] outBytes;

        //
        // If the consumer requested an ico format return the stream as is without any conversion
        //
        if (icoFormat.ToLower() == "ico")
        {
            outBytes = mStream.GetBuffer();
            mStream.Dispose();
            return Convert.ToBase64String(outBytes);
        }

        //
        // Store the stream in an Icon object, this will solve the transparency problem
        //
        mStream.Position = 0;
        Icon iconStream = new Icon(mStream, depth, depth);

        //
        // Retrieve the icon as bitmap for conversion
        //
        Bitmap bitMap = iconStream.ToBitmap();
        ImageFormat imgFormat = ImageFormat.Png;
        
        switch (icoFormat.ToLower())
        {
            case "png":
                imgFormat = ImageFormat.Png;
                break;
            case "gif":
                imgFormat = ImageFormat.Gif;
                break;
            case "tiff":
                imgFormat = ImageFormat.Tiff;
                break;
        }

        //
        // Save the icon in the requested format to an icon to be sent to the consumer
        //
        MemoryStream oMStream = new MemoryStream();
        bitMap.Save(oMStream, imgFormat);
        outBytes = oMStream.GetBuffer();
        mStream.Dispose();
        oMStream.Close();
        oMStream.Dispose();
        bitMap.Dispose();

        //
        // Return the icon as base64 string
        //
        return Convert.ToBase64String(outBytes);
    }
}
#endregion