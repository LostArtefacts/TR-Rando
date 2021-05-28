using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TextureExport
{
    class Program
    {
        enum Mode
        {
            Png, Html, Segments
        }

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Contains("?"))
            {
                Usage();
                return;
            }

            TR2Level instance;
            TR2LevelReader reader = new TR2LevelReader();

            Mode mode = Mode.Png;
            if (args.Length > 1)
            {
                string arg = args[1].ToLower();
                if (arg == "html")
                {
                    mode = Mode.Html;
                }
                else if (arg == "segments")
                {
                    mode = Mode.Segments;
                }
            }

            if (args[0].ToLower().EndsWith(".tr2"))
            {
                if (File.Exists(args[0]))
                {
                    instance = reader.ReadLevel(args[0]);

                    ExportAllTextures(args[0], instance, mode);
                }
            }
            else if (args[0] == "gold")
            {
                foreach (string lvl in LevelNames.AsListGold)
                {
                    if (File.Exists(lvl))
                    {
                        instance = reader.ReadLevel(lvl);

                        ExportAllTextures(lvl, instance, mode);
                    }
                }
            }
            else
            {
                foreach (string lvl in LevelNames.AsList)
                {
                    if (File.Exists(lvl))
                    {
                        instance = reader.ReadLevel(lvl);

                        ExportAllTextures(lvl, instance, mode);
                    }
                }
            }
        }

        static void ExportAllTextures(string lvl, TR2Level inst, Mode mode)
        {
            switch (mode)
            {
                case Mode.Html:
                    new HtmlTileBuilder(inst).ExportAllTexturesToHtml(lvl);
                    break;
                case Mode.Segments:
                    new HtmlTileBuilder(inst).ExportAllTextureSegments(lvl);
                    break;
                default:
                    ExportAllTexturesToPng(lvl, inst);
                    break;
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

        static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: TextureExport [orig | gold | *.tr2] [png | html | segments]");
            Console.WriteLine();

            Console.WriteLine("Target Levels");
            Console.WriteLine("\torig     - The original TR2 levels. Default option.");
            Console.WriteLine("\tgold     - The TR2 Golden Mask levels.");
            Console.WriteLine("\t*.tr2    - Use a specific level file.");
            Console.WriteLine();

            Console.WriteLine("Export Mode");
            Console.WriteLine("\tpng      - Export each texture tile to PNG. Default Option.");
            Console.WriteLine("\thtml     - Export all tiles to a single HTML document.");
            Console.WriteLine("\tsegments - Export each object and sprite texture to individual PNG files.");
            Console.WriteLine();
            
            Console.WriteLine("Examples");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all original level tiles to PNG.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport gold html");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all Golden Mask level tiles to HTML.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport BOAT.TR2");
            Console.ResetColor();
            Console.WriteLine("\t\tExport the Venice level tiles to PNG.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport FLOATING.TR2 segments");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all object and sprite textures from Floating Islands to individual PNGs.");
            Console.WriteLine("\t\tA sub-directory will be created using the level name to store the files.");
            Console.WriteLine();
        }
    }
}