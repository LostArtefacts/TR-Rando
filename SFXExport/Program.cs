using TRLevelControl;
using TRLevelControl.Model;

namespace SFXExport;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2 || args[0].Contains('?'))
        {
            Usage();
            return;
        }

        string file = args[0];
        TRGameVersion version = Enum.Parse<TRGameVersion>(args[1]);
        if (!Enum.IsDefined(typeof(TRGameVersion), version))
        {
            Console.WriteLine("Unrecognised game verion.");
            return;
        }

        if (Directory.Exists(version.ToString()))
        {
            Directory.Delete(version.ToString(), true);
        }
        Directory.CreateDirectory(version.ToString());

        bool remaster = args.Length > 2 && args[2].ToUpper() == "REMASTER";

        switch (Path.GetExtension(file).ToUpper())
        {
            case ".SFX":
                ExtractFromSFX(file, version, remaster);
                break;
            case ".PHD":
                ExtractFromPHD(file, version);
                break;
            default:
                Console.WriteLine("Unsupported file.");
                break;
        }
    }

    private static void ExtractFromSFX(string file, TRGameVersion version, bool remaster)
    {
        int sample = 0;
        using BinaryReader reader = new(File.Open(file, FileMode.Open));

        if (remaster)
        {
            // Header remains unknown for now
            reader.BaseStream.Position = version == TRGameVersion.TR1 ? 0x200 : 0x2E4;
        }
        
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            uint[] header = new uint[11];
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = reader.ReadUInt32();
            }

            using BinaryWriter writer = new(File.Create(Path.Combine(version.ToString(), sample++ + ".wav")));
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

    private static void ExtractFromPHD(string file, TRGameVersion version)
    {
        TR1Level level = new TR1LevelControl().Read(file);
        foreach (var (sfxID, effect) in level.SoundEffects)
        {
            for (int i = 0; i < effect.Samples.Count; i++)
            {
                string path = Path.Combine(version.ToString(), $"{((int)sfxID).ToString().PadLeft(3, '0')}_{i}.wav");
                File.WriteAllBytes(path, effect.Samples[i]);
            }
        }
    }

    private static void Usage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage: SFXExport [*.SFX|*.PHD] [TR1|TR2|TR3] [REMASTER|CLASSIC]");
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
