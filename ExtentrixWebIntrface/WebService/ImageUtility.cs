using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.IO;


class ImageUtility
{



    #region utility method

    /*
         *
         */

    public static Image resizeImage(Image img, int new_width, int new_height)
    {
        //
        // resample using ResamplingService
        //
        /* Size nSize = new Size(new_width, new_height);
         ResamplingService resamplingService = new ResamplingService();

         resamplingService.Filter = ResamplingFilters.CubicBSpline;

         ushort[][,] input = ConvertBitmapToArray((Bitmap)img);

         ushort[][,] output = resamplingService.Resample(input, nSize.Width, nSize.Height);

         Image imgCustom = (Image)ConvertArrayToBitmap(output);
         return imgCustom;*/

        //
        // resample using GDI+
        //
        Size nSize = new Size(new_width, new_height);
        Image imgGdi = new Bitmap(nSize.Width, nSize.Height);

        Graphics grfx = Graphics.FromImage(imgGdi);

        grfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // necessary setting for proper work with image borders
        grfx.PixelOffsetMode = PixelOffsetMode.HighQuality;

        grfx.DrawImage(img,
          new Rectangle(new Point(0, 0), nSize), new Rectangle(new Point(0, 0), img.Size),
          GraphicsUnit.Pixel);

        grfx.Dispose();
        ((Bitmap)imgGdi).MakeTransparent();

        /*Bitmap b = new Bitmap(img, new_width, new_height);
        Graphics g = Graphics.FromImage((Image)b);


        g.DrawImage(img, 0, 0, new_width, new_height);
        g.Dispose();

        b.MakeTransparent(Color.Transparent);

        return (Image)b;*/
        return imgGdi;
    }


    #region Private Methods

    /// <summary>
    /// Converts Bitmap to array. Supports only Format32bppArgb pixel format.
    /// </summary>
    /// <param name="bmp">Bitmap to convert.</param>
    /// <returns>Output array.</returns>
    /*private static ushort[][,] ConvertBitmapToArray(Bitmap bmp)
    {

        ushort[][,] array = new ushort[4][,];

        for (int i = 0; i < 4; i++)
            array[i] = new ushort[bmp.Width, bmp.Height];

        BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int nOffset = (bd.Stride - bd.Width * 4);

        unsafe
        {

            byte* p = (byte*)bd.Scan0;

            for (int y = 0; y < bd.Height; y++)
            {
                for (int x = 0; x < bd.Width; x++)
                {

                    array[3][x, y] = (ushort)p[3];
                    array[2][x, y] = (ushort)p[2];
                    array[1][x, y] = (ushort)p[1];
                    array[0][x, y] = (ushort)p[0];

                    p += 4;
                }

                p += nOffset;
            }
        }

        bmp.UnlockBits(bd);

        return array;
    }
    */
    /// <summary>
    /// Converts array to Bitmap. Supports only Format32bppArgb pixel format.
    /// </summary>
    /// <param name="array">Array to convert.</param>
    /// <returns>Output Bitmap.</returns>
    /* private static Bitmap ConvertArrayToBitmap(ushort[][,] array)
     {

         int width = array[0].GetLength(0);
         int height = array[0].GetLength(1);

         Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

         BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
         int nOffset = (bd.Stride - bd.Width * 4);

         unsafe
         {

             byte* p = (byte*)bd.Scan0;

             for (int y = 0; y < height; y++)
             {
                 for (int x = 0; x < width; x++)
                 {

                     p[3] = (byte)Math.Min(Math.Max(array[3][x, y], Byte.MinValue), Byte.MaxValue);
                     p[2] = (byte)Math.Min(Math.Max(array[2][x, y], Byte.MinValue), Byte.MaxValue);
                     p[1] = (byte)Math.Min(Math.Max(array[1][x, y], Byte.MinValue), Byte.MaxValue);
                     p[0] = (byte)Math.Min(Math.Max(array[0][x, y], Byte.MinValue), Byte.MaxValue);

                     p += 4;
                 }

                 p += nOffset;
             }
         }

         bmp.UnlockBits(bd);

         return bmp;
     }
     */
    #endregion


    /* public static Image resizeImage(Image img, int new_width, int new_height)
        {
            //Icon b = new Icon( (img,new_width,new_height);
            Bitmap b = new Bitmap(img, new_width, new_height);
            // Bitmap bb = new Bitmap(
            Graphics g = Graphics.FromImage((Image)b);


            g.DrawImage(img, 0, 0, new_width, new_height);
            g.Dispose();
            Color backColor = b.GetPixel(0, 0);
            Color transColor = Color.FromArgb(13, 11, 12);
            //color depth
            //RGB(13,11,12)
            b.MakeTransparent();
            b.MakeTransparent(transColor);
            b.MakeTransparent(backColor);
            //b.MakeTransparent();
            return (Image)b;
        }*/

    public static Image resizeImage(string path, int new_width, int new_height)
    {
        Icon icon = new Icon(path, new_width, new_height);
        Bitmap b = icon.ToBitmap();// new Bitmap(new_width, new_height);
        Graphics g = Graphics.FromImage((Image)b);

        g.DrawImage(b, 0, 0, new_width, new_height);
        g.Dispose();
        b.MakeTransparent();
        b.MakeTransparent(Color.Black);
        return (Image)b;
    }




    /// <summary>
    /// converts from input stream to output bytearray
    /// inspired from: http://www.eggheadcafe.com/articles/20030515.asp
    /// </summary>
    /// <param name="ImageSavePath"></param>
    /// <param name="MaxSideSize"></param>
    /// <param name="Buffer"></param>
    /// <returns></returns>
    public static byte[] ResizeFromStream(string fileName, int MaxSideSize, Stream Buffer)
    {
        byte[] byteArray = null;  // really make this an error gif

        try
        {




            Bitmap bitMap = new Bitmap(Buffer);
            int intOldWidth = bitMap.Width;
            int intOldHeight = bitMap.Height;

            int intNewWidth;
            int intNewHeight;

            int intMaxSide;

            if (intOldWidth >= intOldHeight)
            {
                intMaxSide = intOldWidth;
            }
            else
            {
                intMaxSide = intOldHeight;
            }

            if (intMaxSide > MaxSideSize)
            {
                //set new width and height
                double dblCoef = MaxSideSize / (double)intMaxSide;
                intNewWidth = Convert.ToInt32(dblCoef * intOldWidth);
                intNewHeight = Convert.ToInt32(dblCoef * intOldHeight);
            }
            else
            {
                intNewWidth = intOldWidth;
                intNewHeight = intOldHeight;
            }

            Size ThumbNailSize = new Size(intNewWidth, intNewHeight);
            System.Drawing.Image oImg = System.Drawing.Image.FromStream(Buffer);
            System.Drawing.Image oThumbNail = new Bitmap
                (ThumbNailSize.Width, ThumbNailSize.Height);
            Graphics oGraphic = Graphics.FromImage(oThumbNail);
            oGraphic.CompositingQuality = CompositingQuality.HighQuality;
            oGraphic.SmoothingMode = SmoothingMode.HighQuality;
            oGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Rectangle oRectangle = new Rectangle
                (0, 0, ThumbNailSize.Width, ThumbNailSize.Height);

            oGraphic.DrawImage(oImg, oRectangle);

            //string fileName = Context.Server.MapPath("~/App_Data/") + "test1.jpg";
            //oThumbNail.Save(fileName, ImageFormat.Jpeg);
            MemoryStream ms = new MemoryStream();
            oThumbNail.Save(ms, ImageFormat.Jpeg);
            byteArray = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(byteArray, 0, Convert.ToInt32(ms.Length));

            oGraphic.Dispose();
            oImg.Dispose();
            ms.Close();
            ms.Dispose();
        }
        catch (Exception)
        {
            int newSize = MaxSideSize - 20;
            Bitmap bitMap = new Bitmap(newSize, newSize);
            Graphics g = Graphics.FromImage(bitMap);
            g.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, newSize, newSize));

            Font font = new Font("Courier", 8);
            SolidBrush solidBrush = new SolidBrush(Color.Red);
            g.DrawString("Failed File", font, solidBrush, 10, 5);
            g.DrawString(fileName, font, solidBrush, 10, 50);

            MemoryStream ms = new MemoryStream();
            bitMap.Save(ms, ImageFormat.Jpeg);
            byteArray = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(byteArray, 0, Convert.ToInt32(ms.Length));

            ms.Close();
            ms.Dispose();
            bitMap.Dispose();
            solidBrush.Dispose();
            g.Dispose();
            font.Dispose();

        }
        return byteArray;
    }


    public static byte[] ResizeImage(int MaxWidth, int MaxHeight, Image fullImg, string format)
    {

        Bitmap OrigImg = (System.Drawing.Bitmap)fullImg.Clone(); // clone the original image
        Size ResizedDimensions = GetDimensions(MaxWidth, MaxHeight, ref OrigImg); // generate the new resized dimension
        Bitmap NewImg = new Bitmap(OrigImg, ResizedDimensions); // create a new image accordingly
        return GetNewImageBytes(NewImg, format); // call our saveFile method
    }


    public static byte[] ResizeImage2(int MaxWidth, int MaxHeight, Image fullImg, string format)
    {

        Bitmap OrigImg = (System.Drawing.Bitmap)fullImg.Clone(); // clone the original image
        Size ResizedDimensions = new Size(MaxWidth, MaxHeight); // generate the new resized dimension
        Bitmap NewImg = new Bitmap(OrigImg, ResizedDimensions); // create a new image accordingly
        return GetNewImageBytes(NewImg, format); // call our saveFile method
    }

    public static Size GetDimensions(int MaxWidth, int MaxHeight, ref Bitmap Img)
    {
        int Height; int Width; float Multiplier;
        Height = Img.Height; Width = Img.Width; // retrieve original height and width
        if (Height <= MaxHeight && Width <= MaxWidth) // if the original dimensions are smaller than the new dimensions
            return new Size(Width, Height); // return the original dimensions [do nothing on size]
        Multiplier = (float)((float)MaxWidth / (float)Width); // otherwise we calculate the multiplier (maxwidth to width ratio)
        if ((Height * Multiplier) <= MaxHeight) // generate the new height if that's the case - if it must be resized
        {
            Height = (int)(Height * Multiplier);
            return new Size(MaxWidth, Height);
        }
        Multiplier = (float)MaxHeight / (float)Height; // calculate another multiplier (maxheight to height ratio) 
        Width = (int)(Width * Multiplier); // generate the new width using the multiplier
        return new Size(Width, MaxHeight); // return the newly generated dimensions as Size
    }
    private static byte[] GetNewImageBytes(Image imageToConvert, string format)
    {
        EncoderParameters codecParams = new EncoderParameters(1); // configure our encoders
        codecParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L); // for high quality
        ImageCodecInfo[] encoders;
        encoders = ImageCodecInfo.GetImageEncoders();
       // bool success; success = false; // boolean variable for error handling
        byte[] imageByteArray;
        try
        {


            switch (format.ToUpper())
            {
                // save the files using the appropiate encoders
                case "PNG":
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // if (formatOfImage.Equals(ImageFormat.Icon))
                        // Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);

                        //else
                        imageToConvert.Save(ms, encoders[4], codecParams);
                        //imageToConvert.Save("test.ico");
                        imageByteArray = ms.ToArray();
                    }
                    return imageByteArray;
                    //break;

                case "ICO":

                    // if (formatOfImage.Equals(ImageFormat.Icon))
                    // Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);

                    //else
                    using (MemoryStream ms = new MemoryStream())
                    {

                        Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);

                        //imageToConvert.Save("test.ico");
                        imageByteArray = ms.ToArray();
                    }


                    // imageToConvert.Save(ms, encoders[4], codecParams);


                    return imageByteArray;
                    //break;
                // img.Save(NewFileName, encoders[4], codecParams); success = true; break;
                case "BMP":
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // if (formatOfImage.Equals(ImageFormat.Icon))
                        // Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);

                        //else
                        imageToConvert.Save(ms, encoders[0], codecParams);
                        //imageToConvert.Save("test.ico");
                        imageByteArray = ms.ToArray();
                    }
                    return imageByteArray;
                   // break;
                // img.Save(
                //img.Save(NewFileName, encoders[0], codecParams); success = true; break;
                case "JPEG":
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // if (formatOfImage.Equals(ImageFormat.Icon))
                        // Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);

                        //else
                        imageToConvert.Save(ms, encoders[1], codecParams);
                        //imageToConvert.Save("test.ico");
                        imageByteArray = ms.ToArray();
                    }
                    return imageByteArray;
                   // break;
                // img.Save(NewFileName, encoders[1], codecParams); success = true; break;
                case "GIF":
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //OctreeQuantizer quantizer = new OctreeQuantizer(255, 8); // 255 colors, 8 color bits
                        // using (Image quantized = quantizer.Quantize(img)) // quantize our image using the said settings
                        // {
                        imageToConvert.Save(ms, encoders[2], codecParams); // save the quantized image
                        //}
                        imageByteArray = ms.ToArray();
                    }
                    return imageByteArray;
                   // break;


                case "TIFF":
                    using (MemoryStream ms = new MemoryStream())
                    {
                        //OctreeQuantizer quantizer = new OctreeQuantizer(255, 8); // 255 colors, 8 color bits
                        // using (Image quantized = quantizer.Quantize(img)) // quantize our image using the said settings
                        // {
                        imageToConvert.Save(ms, encoders[3], codecParams); // save the quantized image
                        //}
                        imageByteArray = ms.ToArray();
                    }
                    return imageByteArray;
                   // break;

                // standard GDI+ conversion (using default quantization) using the following line:
                // img.Save(newName, encoders[2], codecParams); success = 1; break;  
                // however, we'll use the OctreeQuantizer found in BetterImageProcessorQuantization.dll
                //OctreeQuantizer quantizer = new OctreeQuantizer(255, 8); // 255 colors, 8 color bits
                //using (Image quantized = quantizer.Quantize(img)) // quantize our image using the said settings
                //{
                //    quantized.Save(NewFileName, encoders[2], codecParams); // save the quantized image
                //}
                //success = true; break;
            }
        }
        catch // if there was an error then the saving process failed - error message box
        {
            // MessageBox.Show("Failed to save image to " + NewFileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //return;
        }
        // if (success == true) // if the saving process succeeded then notify the user
        //MessageBox.Show("Image file saved to " + NewFileName, "Image Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return null;
    }




    public static byte[]
    ConvertImageToByteArray(System.Drawing.Image imageToConvert, ImageFormat formatOfImage)
    {
        byte[] imageByteArray;
        try
        {

            using (MemoryStream ms = new MemoryStream())
            {
                /*if (formatOfImage.Equals(ImageFormat.Icon))
                {
                    Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(ms);
                    FileStream fs = new FileStream("c:\\icoico.ico", FileMode.CreateNew);
                    Icon.FromHandle((((Bitmap)imageToConvert).GetHicon())).Save(fs);
                    fs.Close();
                }
                else*/
                imageToConvert.Save(ms, formatOfImage);
                imageByteArray = ms.ToArray();
            }
        }
        catch (Exception ex)
        {
            string e = ex.Message;
            throw;
        }
        return imageByteArray;
    }

    public static byte[] getImageBytes(String iconData)
    {

        //Save the published application icon to file.
        // 1)Get the encoded bytes from the icon data string
        try
        {

            //Get the encoded byte array
            byte[] bimg = System.Convert.FromBase64String(iconData);
            return bimg;


        }
        catch
        {
            return null;
        }

    }

    public static byte[]
    ConvertImageToByteArray(Icon imageToConvert, ImageFormat formatOfImage)
    {
        byte[] Ret;
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageToConvert.Save(ms);
                //imageToConvert.Save("test.ico");
                Ret = ms.ToArray();
            }
        }
        catch (Exception) { throw; }
        return Ret;
    }

    public static Image ConvertByteArrayToImage(byte[] byteArray)
    {
        if (byteArray != null)
        {
            MemoryStream ms = new MemoryStream(byteArray, 0,
            byteArray.Length);
            ms.Write(byteArray, 0, byteArray.Length);
            return Image.FromStream(ms, true);
        }
        return null;
    }


    public static byte[] GetImageBytes(string strImageName)
    {
        ImageClass ic = new ImageClass();
        FileStream fs = new FileStream(strImageName, FileMode.OpenOrCreate, FileAccess.Read);
        Byte[] img = new Byte[fs.Length];
        try
        {
            fs.Read(img, 0, Convert.ToInt32(fs.Length));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
        fs.Close();
        ic.myImage = img;
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, ic);
        //return ms.ToArray();
        return img;
    }


    public static ImageClass GetImage(string strImageName)
    {
        ImageClass ic = new ImageClass();
        FileStream fs = new FileStream(strImageName, FileMode.OpenOrCreate, FileAccess.Read);
        Byte[] img = new Byte[fs.Length];
        try
        {
            fs.Read(img, 0, Convert.ToInt32(fs.Length));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
        fs.Close();
        ic.myImage = img;
        return ic;
    }

    public static Image createImage(byte[] byteArrayIn)
    {
        try
        {

            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms, true);

            return returnImage;
        }
        catch
        {
            return null;
        }

    }

    /* private void ResizeImage()
     {
         Image imageToResize = Image.FromFile(@imagePath);
         Image resizedImage = ScaleByPercent(imageToResize, 50);
         resizedImage.Save("c:/image");
     }*/


    public static Image ScaleByPercent(Image imgPhoto, int Percent)
    {
        Bitmap bitmap = (Bitmap)imgPhoto; ;
        // bitmap.MakeTransparent
        float nPercent = ((float)Percent / 100);

        int sourceWidth = imgPhoto.Width;
        int sourceHeight = imgPhoto.Height;
        int sourceX = 0;
        int sourceY = 0;

        int destX = 0;
        int destY = 0;
        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);

        Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                                 PixelFormat.Format24bppRgb);
        bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                imgPhoto.VerticalResolution);

        Graphics grPhoto = Graphics.FromImage(bmPhoto);
        grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

        grPhoto.DrawImage(imgPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);

        grPhoto.Dispose();
        // Loop through the images pixels to reset color.

        /* for (int x = 0; x < bmPhoto.Width; x++)
          {

              for (int y = 0; y < bmPhoto.Height; y++)
              {

                  Color pixelColor = bmPhoto.GetPixel(x, y);

                  Color newColor = Color.FromArgb(pixelColor.R, 0, 0);

                  bmPhoto.SetPixel(x, y, newColor);

              }
          }*/
        //MakeTransparent(Color.Black);
        bmPhoto.MakeTransparent(Color.Black);
        bmPhoto.MakeTransparent();
        return bmPhoto;
    }


    #endregion




}


[Serializable]
public class ImageClass
{
    public byte[] myImage;
    public ImageClass()
    {
    }
}





