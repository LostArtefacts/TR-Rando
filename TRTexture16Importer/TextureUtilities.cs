using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TRTexture16Importer
{
    public static class TextureUtilities
    {
        private static readonly int _tileSize = 256;

        public static ushort[] ImportFrom32PNG(string filename)
        {
            using (Bitmap texture = new Bitmap(filename))
            {
                return ImportFromBitmap(texture);
            }
        }

        public static ushort[] ImportFromBitmap(Bitmap texture)
        {
            ushort[] convertedPixels = new ushort[_tileSize * _tileSize];

            Debug.Assert(texture.PixelFormat == PixelFormat.Format32bppArgb);
            Debug.Assert(texture.Width == _tileSize);
            Debug.Assert(texture.Height == _tileSize);

            for (int y = 0; y < _tileSize; y++)
            {
                for (int x = 0; x < _tileSize; x++)
                {
                    convertedPixels[y * _tileSize + x] = ColorToRGB555(texture.GetPixel(x, y));
                }
            }

            return convertedPixels;
        }

        private static ushort ColorToRGB555(Color pixel)
        {
            ushort newPixel;

            //Get the 32bpp values (byte each)
            byte Red = TextileToBitmapConverter.From32BPP(pixel.R);
            byte Green = TextileToBitmapConverter.From32BPP(pixel.G);
            byte Blue = TextileToBitmapConverter.From32BPP(pixel.B);
            byte Alpha = Math.Min(pixel.A, (byte)1);

            //Perform bit shifting
            ushort sRed = (ushort)(Red << 10);
            ushort sGreen = (ushort)(Green << 5);
            ushort sAlpha = (ushort)(Alpha << 15);

            newPixel = (ushort)(sRed | sGreen | Blue | sAlpha);

            return newPixel;
        }

        public static Bitmap ToBitmap(this TRTexImage8 tex, TRColour[] palette)
        {
            Bitmap bmp = new Bitmap(_tileSize, _tileSize, PixelFormat.Format32bppRgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            List<byte> pixelCollection = new List<byte>();
            foreach (byte colourIndex in tex.Pixels)
            {
                TRColour c = palette[colourIndex];
                pixelCollection.Add((byte)(4 * c.Blue));
                pixelCollection.Add((byte)(4 * c.Green));
                pixelCollection.Add((byte)(4 * c.Red));
                pixelCollection.Add((byte)(colourIndex == 0 ? 0 : 0xFF)); // The first entry in the palette is used for transparency
            }

            Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
            bmp.UnlockBits(bitmapData);

            return bmp;
        }

        public static Bitmap ToBitmap(this TRTexImage16 tex)
        {
            Bitmap bmp = new Bitmap(_tileSize, _tileSize, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            List<byte> pixelCollection = new List<byte>();

            foreach (Textile16Pixel px in tex.To32BPPFormat())
            {
                pixelCollection.AddRange(px.RGB32);
            }

            Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
            bmp.UnlockBits(bitmapData);

            return bmp;
        }
    }
}