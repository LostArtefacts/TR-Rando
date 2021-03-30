using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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

            bool htmlMode = args.Length > 1 && args[1].ToLower() == "html";

            if (args[0].ToLower().EndsWith(".tr2"))
            {
                instance = reader.ReadLevel(args[0]);

                ExportAllTextures(args[0], instance, htmlMode);
            }
            else if (args[0] == "gold")
            {
                foreach (string lvl in LevelNames.AsListGold)
                {
                    instance = reader.ReadLevel(lvl);

                    ExportAllTextures(lvl, instance, htmlMode);
                }
            }
            else if (args[0] == "orig")
            {
                foreach (string lvl in LevelNames.AsList)
                {
                    instance = reader.ReadLevel(lvl);

                    ExportAllTextures(lvl, instance, htmlMode);
                }
            }
        }

        static void ExportAllTextures(string lvl, TR2Level inst, bool htmlMode)
        {
            if (htmlMode)
            {
                using (HtmlTileBuilder builder = new HtmlTileBuilder(inst))
                {
                    builder.ExportAllTexturesToHtml(lvl);
                }
            }
            else
            {
                ExportAllTexturesToPng(lvl, inst);
            }
        }

        static void ExportAllTexturesToPng(string lvl, TR2Level inst)
        {
            const int width = 256;
            const int height = 256;

            int index = 0;

            foreach (TRTexImage16 tex in inst.Images16)
            {
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

                List<byte> pixelCollection = new List<byte>();
                
                foreach (Textile16Pixel px in tex.To32BPPFormat())
                {
                    pixelCollection.AddRange(px.RGB32);
                }

                Marshal.Copy(pixelCollection.ToArray(), 0, bitmapData.Scan0, pixelCollection.Count);
                bmp.UnlockBits(bitmapData);
                bmp.Save(lvl + index + ".png");

                index++;
            }
        }
    }
}