using System;
using System.IO;
using TRLevelReader;
using TRLevelReader.Model;

namespace SFXExport
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args[0].Contains("?"))
            {
                Usage();
                return;
            }

            string file = args[0];
            string exportDir = args[1];
            if (Directory.Exists(exportDir))
            {
                Directory.Delete(exportDir, true);
            }
            Directory.CreateDirectory(exportDir);

            switch (Path.GetExtension(file).ToUpper())
            {
                case ".SFX":
                    ExtractFromSFX(file, exportDir);
                    break;
                case ".PHD":
                    ExtractFromPHD(file, exportDir);
                    break;
                default:
                    Console.WriteLine("Unsupported file.");
                    break;
            }
        }

        private static void ExtractFromSFX(string file, string exportDir)
        {
            int sample = 0;
            using (BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    uint[] header = new uint[11];
                    for (int i = 0; i < header.Length; i++)
                    {
                        header[i] = reader.ReadUInt32();
                    }

                    using (BinaryWriter writer = new BinaryWriter(File.Create(Path.Combine(exportDir, sample++ + ".wav"))))
                    {
                        for (int i = 0; i < header.Length; i++)
                        {
                            writer.Write(header[i]);
                        }

                        uint dataLength = header[10];
                        for (int i = 0; i < dataLength; i++)
                        {
                            writer.Write(reader.ReadByte());
                        }
                    }
                }
            }
        }

        private static void ExtractFromPHD(string file, string exportDir)
        {
            TRLevel level = new TR1LevelReader().ReadLevel(file);
            for (int i = 0; i < level.NumSampleIndices; i++)
            {
                uint sampleStart = level.SampleIndices[i];
                uint sampleEnd = i < level.NumSampleIndices - 1 ? level.SampleIndices[i + 1] : (uint)level.Samples.Length;
                if (sampleEnd > level.Samples.Length)
                {
                    sampleEnd = (uint)level.Samples.Length;
                }

                using (BinaryWriter writer = new BinaryWriter(File.Create(Path.Combine(exportDir, i + ".wav"))))
                {
                    for (uint j = sampleStart; j < sampleEnd; j++)
                    {
                        writer.Write(level.Samples[j]);
                    }
                }
            }
        }

        private static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage: SFXExport [*.SFX|*.PHD] [EXPORT_DIR]");
            Console.WriteLine();

            Console.WriteLine("Example");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tSFXExport MAIN.SFX TR2");
            Console.ResetColor();
            Console.WriteLine("\t\tExtract each sound effect from the SFX file and create a sample file for each in the TR2 directory.");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\tSFXExport LEVEL1.PHD TR1");
            Console.ResetColor();
            Console.WriteLine("\t\tExtract each sound effect from Caves and create a sample file for each in the TR1 directory.");
            Console.WriteLine();
        }
    }
}