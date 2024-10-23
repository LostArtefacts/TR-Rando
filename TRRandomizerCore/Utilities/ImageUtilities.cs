using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TRImageControl;

namespace TRRandomizerCore.Utilities;

public static class ImageUtilities
{
    // Ideally we will eliminate all Bitmap requirements to support cross-platform, but in the
    // meantime we leave these utility functions in the core library rather than putting the
    // Windows-only dependency on other libraries as well.

    public static TRImage BitmapToImage(Bitmap bitmap)
    {
        Size size = bitmap.Size;
        TRImage image = new(size);

        BitmapData bd = bitmap.LockBits(new(0, 0, size.Width, size.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
        int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        int byteCount = bd.Stride * size.Height;
        byte[] pixels = new byte[byteCount];
        IntPtr ptrFirstPixel = bd.Scan0;
        Marshal.Copy(ptrFirstPixel, pixels, 0, byteCount);

        int endX = size.Width * bytesPerPixel;
        for (int y = 0; y < size.Height; y++)
        {
            int currentLine = y * bd.Stride;
            for (int x = 0; x < endX; x += bytesPerPixel)
            {
                uint c = 0;
                for (int i = 0; i < bytesPerPixel; i++)
                {
                    c |= (uint)(pixels[currentLine + x + i] << (i * 8));
                }

                image[x / bytesPerPixel, y] = c;
            }
        }

        bitmap.UnlockBits(bd);

        return image;
    }

    public static Bitmap ImageToBitmap(TRImage image)
    {
        List<byte> pixels = new(image.Pixels.Length * 4);
        for (int i = 0; i < image.Pixels.Length; i++)
        {
            uint pixel = image.Pixels[i];
            byte a = (byte)((pixel & 0xFF000000) >> 24);
            byte r = (byte)((pixel & 0xFF0000) >> 16);
            byte g = (byte)((pixel & 0xFF00) >> 8);
            byte b = (byte)(pixel & 0xFF);

            pixels.Add(b);
            pixels.Add(g);
            pixels.Add(r);
            pixels.Add(a);
        }

        Bitmap bitmap = new(image.Size.Width, image.Size.Height, PixelFormat.Format32bppArgb);
        BitmapData bitmapData = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        Marshal.Copy(pixels.ToArray(), 0, bitmapData.Scan0, pixels.Count);
        bitmap.UnlockBits(bitmapData);

        return bitmap;
    }
}
