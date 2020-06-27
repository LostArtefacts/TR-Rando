using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Helpers;

namespace TRTexture16Importer
{
    public static class T16Importer
    {
        public static ushort[] ImportFrom32PNG(string filename)
        {
            Bitmap texture = new Bitmap(filename);

            ushort[] convertedPixels = new ushort[256 * 256];

            Debug.Assert(texture.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Debug.Assert(texture.Width == 256);
            Debug.Assert(texture.Height == 256);

            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x++)
                {
                    convertedPixels[y * 256 + x] = ColorToRGB555(texture.GetPixel(x, y));
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
    }
}
