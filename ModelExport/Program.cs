using TRModelTransporter.Utilities;

namespace ModelExport;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1 || args[0].Contains('?'))
        {
            Usage();
            return;
        }

        bool exportSegments = args.Length > 1 && args[1].ToLower().Contains("segments");
        switch (args[0].ToLower())
        {
            case "tr1":
                TR1Export(exportSegments);
                break;
            case "tr2":
                TR2Export(exportSegments);
                break;
            case "tr3":
                TR3Export(exportSegments);
                break;
        }
    }

    static void TR1Export(bool exportSegments)
    {
        MassTR1ModelExporter exporter = new();
        string exportFolder = @"TR1\Models";
        string segmentFolder = exportSegments ? @"TR1\ModelSegments" : null;

        exporter.Export(Directory.GetCurrentDirectory(), exportFolder, segmentFolder);
    }

    static void TR2Export(bool exportSegments)
    {
        MassTR2ModelExporter exporter = new();
        string exportFolder = @"TR2\Models";
        string segmentFolder = exportSegments ? @"TR2\ModelSegments" : null;

        exporter.Export(Directory.GetCurrentDirectory(), exportFolder, segmentFolder);
    }

    static void TR3Export(bool exportSegments)
    {
        MassTR3ModelExporter exporter = new();
        string exportFolder = @"TR3\Models";
        string segmentFolder = exportSegments ? @"TR3\ModelSegments" : null;

        exporter.Export(Directory.GetCurrentDirectory(), exportFolder, segmentFolder);
    }

    private static void Usage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage: ModelExport [tr1 | tr2 | tr3] [segments]");
        Console.WriteLine();

        Console.WriteLine("Example");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\tModelExport tr1");
        Console.ResetColor();
        Console.WriteLine("\t\tExport defined models for TR1.");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\tModelExport tr2 segments");
        Console.ResetColor();
        Console.WriteLine("\t\tExport defined models for TR2 and include all individual segments for each model.");
        Console.WriteLine();
    }
}
