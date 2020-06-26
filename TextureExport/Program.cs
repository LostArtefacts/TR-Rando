using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TextureExport
{
    class Program
    {
        static void Main(string[] args)
        {
            TR2Level instance;
            TR2LevelReader reader = new TR2LevelReader();

            foreach (string lvl in LevelNames.AsList)
            {
                instance = reader.ReadLevel(lvl);

                ExportAllTexturesToPng(lvl, instance);
            }
        }

        static void ExportAllTexturesToPng(string lvl, TR2Level inst)
        {
            const int width = 256;
            const int height = 256;

            int index = 0;

            foreach (TRTexImage16 tex in inst.Images16)
            {
                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

                List<byte> pixelCollection = new List<byte>();
                
                foreach (Textile16Pixel px in tex.To32BPPFormat())
                {
                    pixelCollection.AddRange(px.RGB32);
                }

                Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
                bmp.UnlockBits(bitmapData);
                bmp.Save(lvl + index + ".bmp");

                index++;
            }
        }
    }
}
