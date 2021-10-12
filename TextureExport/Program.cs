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
            Png, Html, Segments, Faces, Boxes
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
                else if (arg == "faces")
                {
                    mode = Mode.Faces;
                    if (args.Length < 3)
                    {
                        return;
                    }
                }
                else if (arg == "boxes")
                {
                    mode = Mode.Boxes;
                    if (args.Length < 3)
                    {
                        return;
                    }
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
                foreach (string lvl in TR2LevelNames.AsListGold)
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
                foreach (string lvl in TR2LevelNames.AsList)
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
                case Mode.Faces:
                    new FaceMapper(inst).GenerateFaces(lvl.ToLower().Replace(".tr2", "_faced.tr2"), GetFaceRoomArgs());
                    break;
                case Mode.Boxes:
                    new FaceMapper(inst).GenerateBoxes(lvl.ToLower().Replace(".tr2", "_boxed.tr2"), GetFaceRoomArgs());
                    break;
                default:
                    ExportAllTexturesToPng(lvl, inst);
                    break;
            }
        }

        static int[] GetFaceRoomArgs()
        {
            string[] args = Environment.GetCommandLineArgs()[3].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<int> rooms = new List<int>();
            foreach (string rm in args)
            {
                if (int.TryParse(rm.Trim(), out int r))
                {
                    rooms.Add(r);
                }
            }
            return rooms.ToArray();
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
            Console.WriteLine("Usage: TextureExport [orig | gold | *.tr2] [png | html | segments | faces | boxes]");
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
            Console.WriteLine("\tfaces    - Creates a new texture for every face in a room and marks its index (output is LVL_faced.tr2).");
            Console.WriteLine("\tboxes    - Similar to faces, but marks box extents for a list of rooms (output is LVL_boxed.tr2).");
            Console.WriteLine();
            
            Console.WriteLine("Examples");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all original level tiles to PNG.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport orig html");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all original level tiles to HTML.");
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport WALL.TR2 faces 4,32");
            Console.ResetColor();
            Console.WriteLine("\t\tCreates a new texture for every face in rooms 4 and 32 and marks its index on the texture.");
            Console.WriteLine("\t\tThe level will likely be unplayable due to limits but can be viewed in trview.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport WALL.TR2 boxes 4,32");
            Console.ResetColor();
            Console.WriteLine("\t\tCreates a new texture for sectors in rooms 4 and 32, showing the box index for the sector.");
            Console.WriteLine("\t\tThe level will likely be unplayable due to limits but can be viewed in trview.");
            Console.WriteLine();
        }
    }
}