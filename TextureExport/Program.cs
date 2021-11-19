using System;
using System.Collections.Generic;
using System.IO;
using TextureExport.Types;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TextureExport
{
    class Program
    {
        enum Mode
        {
            Png, Html, Segments, Faces, Boxes, Dependencies
        }

        static readonly TR1LevelReader _reader1 = new TR1LevelReader();
        static readonly TR2LevelReader _reader2 = new TR2LevelReader();
        static readonly TR3LevelReader _reader3 = new TR3LevelReader();

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0].Contains("?"))
            {
                Usage();
                return;
            }

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
                else if (arg == "depend")
                {
                    mode = Mode.Dependencies;
                }
            }

            string levelType = args[0].ToLower();

            if (levelType.EndsWith(".phd"))
            {
                ExportAllTextures(args[0], _reader1.ReadLevel(args[0]), mode);
            }
            else if (levelType.EndsWith(".tr2"))
            {
                uint version = DetectVersion(args[0]);
                if (version == Versions.TR2)
                {
                    ExportAllTextures(args[0], _reader2.ReadLevel(args[0]), mode);
                }
                else if (version == Versions.TR3a || version == Versions.TR3b)
                {
                    ExportAllTextures(args[0], _reader3.ReadLevel(args[0]), mode);
                }
            }
            else if (levelType == "tr1")
            {
                foreach (string lvl in TRLevelNames.AsListWithAssault)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader1.ReadLevel(lvl), mode);
                    }
                }
            }
            else if (levelType == "tr1g")
            {
                foreach (string lvl in TRLevelNames.AsListGold)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader1.ReadLevel(lvl), mode);
                    }
                }
            }
            else if (levelType == "tr2g")
            {
                foreach (string lvl in TR2LevelNames.AsListGold)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader2.ReadLevel(lvl), mode);
                    }
                }
            }
            else if (levelType == "tr3")
            {
                foreach (string lvl in TR3LevelNames.AsListWithAssault)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader3.ReadLevel(lvl), mode);
                    }
                }
            }
            else if (levelType == "tr3g")
            {
                foreach (string lvl in TR3LevelNames.AsListGold)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader3.ReadLevel(lvl), mode);
                    }
                }
            }
            else
            {
                foreach (string lvl in TR2LevelNames.AsListWithAssault)
                {
                    if (File.Exists(lvl))
                    {
                        ExportAllTextures(lvl, _reader2.ReadLevel(lvl), mode);
                    }
                }
            }
        }

        static uint DetectVersion(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                return reader.ReadUInt32();
            }
        }

        static void ExportAllTextures(string lvl, TRLevel inst, Mode mode)
        {
            switch (mode)
            {
                case Mode.Png:
                    PngExporter.Export(inst, lvl);
                    break;
                case Mode.Html:
                    HtmlExporter.Export(inst, lvl);
                    break;
                default:
                    Console.WriteLine("{0} mode is not supported for TR1.", mode);
                    break;
            }
        }

        static void ExportAllTextures(string lvl, TR2Level inst, Mode mode)
        {
            switch (mode)
            {
                case Mode.Png:
                    PngExporter.Export(inst, lvl);
                    break;
                case Mode.Html:
                    HtmlExporter.Export(inst, lvl);
                    break;
                case Mode.Segments:
                    SegmentExporter.Export(inst, lvl);
                    break;
                case Mode.Faces:
                    FaceMapper.DrawFaces(inst, lvl, GetRoomArgs());
                    break;
                case Mode.Boxes:
                    FaceMapper.DrawBoxes(inst, lvl, GetRoomArgs());
                    break;
                case Mode.Dependencies:
                    DependencyExporter.Export(inst, lvl);
                    break;
                default:
                    Console.WriteLine("{0} mode is not supported for TR2.", mode);
                    break;
            }
        }

        static void ExportAllTextures(string lvl, TR3Level inst, Mode mode)
        {
            switch (mode)
            {
                case Mode.Png:
                    PngExporter.Export(inst, lvl);
                    break;
                case Mode.Html:
                    HtmlExporter.Export(inst, lvl);
                    break;
                case Mode.Segments:
                    SegmentExporter.Export(inst, lvl);
                    break;
                case Mode.Faces:
                    FaceMapper.DrawFaces(inst, lvl, GetRoomArgs());
                    break;
                case Mode.Dependencies:
                    DependencyExporter.Export(inst, lvl);
                    break;
                default:
                    Console.WriteLine("{0} mode is not supported for TR3.", mode);
                    break;
            }
        }

        static int[] GetRoomArgs()
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

        static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: TextureExport [tr1 | tr1g | tr2 | tr2g | tr3 | tr3g | *.phd | *.tr2] [png | html | segments | faces | boxes | depend]");
            Console.WriteLine();

            Console.WriteLine("Target Levels");
            Console.WriteLine("\ttr1      - The original TR1 levels.");
            Console.WriteLine("\ttr1g     - The TR1 Unfinished Business levels.");
            Console.WriteLine("\ttr2      - The original TR2 levels. Default option.");
            Console.WriteLine("\ttr2g     - The TR2 Golden Mask levels.");
            Console.WriteLine("\ttr3      - The original TR3 levels.");
            Console.WriteLine("\ttr3g     - The TR3 Lost Artefact levels.");
            Console.WriteLine("\t*.phd    - Use a specific TR1 level file.");
            Console.WriteLine("\t*.tr2    - Use a specific TR2/TR3 level file.");
            Console.WriteLine();

            Console.WriteLine("Export Mode");
            Console.WriteLine("\tpng      - Export each texture tile to PNG. Default Option.");
            Console.WriteLine("\thtml     - Export all tiles to a single HTML document.");
            Console.WriteLine("\tsegments - Export each object and sprite texture to individual PNG files.");
            Console.WriteLine("\tfaces    - Create a new texture for every face in a room and mark its index.");
            Console.WriteLine("\tboxes    - Similar to faces, but mark box extents for a list of rooms.");
            Console.WriteLine("\tdepend   - Calculate which textures are shared between models and generate the JSON used in the main randomizer.");
            Console.WriteLine();
            
            Console.WriteLine("Examples");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all TR2 level tiles to PNG.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport tr2 html");
            Console.ResetColor();
            Console.WriteLine("\t\tExport all TR2 level tiles to HTML.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport tr2g html");
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tTextureExport TR3 depend");
            Console.ResetColor();
            Console.WriteLine("\t\tCycle through each TR3 level and work out which textures are shared between models.");
            Console.WriteLine("\t\tJSON files are generated for referencing in the main randomizer. This process will");
            Console.WriteLine("\t\tbe lengthy.");
            Console.WriteLine();
        }
    }
}