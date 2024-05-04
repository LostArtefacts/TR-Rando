using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TRLevelControl;
using TRLevelControl.Model;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer;

public static class TextureUtilities
{
    public static ushort[] ImportFrom32PNG(string filename)
    {
        using Bitmap texture = new(filename);
        return ImportFromBitmap(texture);
    }

    public static ushort[] ImportFromBitmap(Bitmap texture)
    {
        ushort[] convertedPixels = new ushort[TRConsts.TPageSize];

        Debug.Assert(texture.PixelFormat == PixelFormat.Format32bppArgb);
        Debug.Assert(texture.Width == TRConsts.TPageWidth);
        Debug.Assert(texture.Height == TRConsts.TPageHeight);

        for (int y = 0; y < TRConsts.TPageHeight; y++)
        {
            for (int x = 0; x < TRConsts.TPageWidth; x++)
            {
                convertedPixels[y * TRConsts.TPageWidth + x] = texture.GetPixel(x, y).ToRGB555();
            }
        }

        return convertedPixels;
    }

    public static Bitmap ToBitmap(this TRTexImage8 tex, List<TRColour> palette)
    {
        Bitmap bmp = new(TRConsts.TPageWidth, TRConsts.TPageHeight, PixelFormat.Format32bppArgb);
        BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        List<byte> pixelCollection = new();
        foreach (byte colourIndex in tex.Pixels)
        {
            TRColour c = palette[colourIndex];
            pixelCollection.Add(c.Blue);
            pixelCollection.Add(c.Green);
            pixelCollection.Add(c.Red);
            pixelCollection.Add((byte)(colourIndex == 0 ? 0 : 0xFF)); // The first entry in the palette is used for transparency
        }

        Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
        bmp.UnlockBits(bitmapData);

        return bmp;
    }

    public static Bitmap ToBitmap(this TRTexImage16 tex)
    {
        Bitmap bmp = new(TRConsts.TPageWidth, TRConsts.TPageHeight, PixelFormat.Format32bppArgb);
        BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        List<byte> pixelCollection = new();

        foreach (Textile16Pixel px in tex.To32BPPFormat())
        {
            pixelCollection.AddRange(px.RGB32);
        }

        Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
        bmp.UnlockBits(bitmapData);

        return bmp;
    }
}
