using TRDataControl.Utils;

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

        switch (args[0].ToLower())
        {
            case "tr1":
                TR1Export();
                break;
            case "tr2":
                TR2Export();
                break;
            case "tr3":
                TR3Export();
                break;
        }
    }

    static void TR1Export()
    {
        TR1MassExporter exporter = new();
        exporter.Export(Directory.GetCurrentDirectory(), @"TR1\Objects");
    }

    static void TR2Export()
    {
        TR2MassExporter exporter = new();
        exporter.Export(Directory.GetCurrentDirectory(), @"TR2\Objects");
    }

    static void TR3Export()
    {
        TR3MassExporter exporter = new();
        exporter.Export(Directory.GetCurrentDirectory(), @"TR3\Objects");
    }

    private static void Usage()
    {
        Console.WriteLine();
        Console.WriteLine("Usage: ModelExport [tr1 | tr2 | tr3]");
        Console.WriteLine();

        Console.WriteLine("Example");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\tModelExport tr1");
        Console.ResetColor();
        Console.WriteLine("\t\tExport defined models for TR1.");
        Console.WriteLine();
    }
}
