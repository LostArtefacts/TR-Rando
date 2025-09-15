using System.Text;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class TestBase
{
    protected static readonly string _readPath = "Levels/{0}/{1}";

    public static string GetReadPath(string level, TRGameVersion version, bool remastered = false)
    {
        return string.Format(_readPath, version.ToString() + (remastered ? "R" : string.Empty), level);
    }

    public static TR1Level GetTR1TestLevel()
        => GetTR1Level("TEST1.PHD");

    public static TR1Level GetTR1AltTestLevel()
        => GetTR1Level("TEST2.PHD");

    public static TR2Level GetTR2TestLevel()
        => GetTR2Level("TEST1.TR2");

    public static TR2Level GetTR2AltTestLevel()
        => GetTR2Level("TEST2.TR2");

    public static TR3Level GetTR3TestLevel()
        => GetTR3Level("TEST1.TR2");

    public static TR3Level GetTR3AltTestLevel()
        => GetTR3Level("TEST2.TR2");

    public static TR4Level GetTR4TestLevel()
        => GetTR4Level("TEST1.TR4");

    public static TR4Level GetTR4AltTestLevel()
        => GetTR4Level("TEST2.TR4");

    public static TR5Level GetTR5TestLevel()
        => GetTR5Level("TEST1.TRC");

    public static TR5Level GetTR5AltTestLevel()
        => GetTR5Level("TEST2.TRC");

    public static TR1Level GetTR1Level(string level)
    {
        TR1LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR1));
    }

    public static TR2Level GetTR2Level(string level)
    {
        TR2LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR2));
    }

    public static TR3Level GetTR3Level(string level)
    {
        TR3LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR3));
    }

    public static TR4Level GetTR4Level(string level)
    {
        TR4LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR4));
    }

    public static TR5Level GetTR5Level(string level, bool remastered = false)
    {
        TR5LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR5, remastered));
    }

    public static void ReadWriteLevel(string levelName, TRGameVersion version, bool remastered)
    {
        string pathI = GetReadPath(levelName, version, remastered);
        using FileStream dataStream = File.OpenRead(pathI);
        using MemoryStream inputStream = new();
        using MemoryStream outputStream = new();

        dataStream.CopyTo(inputStream);
        byte[] inputData = inputStream.ToArray();
        inputStream.Position = 0;

        ObserverBase observer;
        switch (version)
        {
            case TRGameVersion.TR1:
                observer = new TR1Observer();
                TR1LevelControl control1 = new(observer);
                TR1Level level1 = control1.Read(inputStream);
                control1.Write(level1, outputStream);
                break;
            case TRGameVersion.TR2:
                observer = new TR2Observer();
                TR2LevelControl control2 = new(observer);
                TR2Level level2 = control2.Read(pathI);
                control2.Write(level2, outputStream);
                break;
            case TRGameVersion.TR3:
                bool originalFD = UseOriginalTR3FD(levelName, remastered);
                observer = new TR3Observer(originalFD);
                TR3LevelControl control3 = new(observer);
                TR3Level level3 = control3.Read(pathI);
                control3.Write(level3, outputStream);
                break;
            case TRGameVersion.TR4:
                observer = new TR4Observer(remastered);
                TR4LevelControl control4 = new(observer);
                TR4Level level4 = control4.Read(pathI);
                control4.Write(level4, outputStream);
                break;
            case TRGameVersion.TR5:
                observer = new TR5Observer(remastered);
                TR5LevelControl control5 = new(observer);
                TR5Level level5 = control5.Read(pathI);
                control5.Write(level5, outputStream);
                break;
            default:
                throw new NotImplementedException();
        }

        observer.TestOutput(inputData, outputStream.ToArray());
    }

    private static bool UseOriginalTR3FD(string levelName, bool remastered)
    {
        return levelName == TR3LevelNames.ANTARC
            || (remastered && (levelName == TR3LevelNames.JUNGLE_CUT || levelName == TR3LevelNames.MADUBU));
    }

    public static TR1Level WriteReadTempLevel(TR1Level level)
    {
        using MemoryStream ms = new();
        TR1LevelControl control = new();
        control.Write(level, ms);
        return control.Read(new MemoryStream(ms.ToArray()));
    }

    public static TR2Level WriteReadTempLevel(TR2Level level)
    {
        using MemoryStream ms = new();
        TR2LevelControl control = new();
        control.Write(level, ms);
        return control.Read(new MemoryStream(ms.ToArray()));
    }

    public static TR3Level WriteReadTempLevel(TR3Level level)
    {
        using MemoryStream ms = new();
        TR3LevelControl control = new();
        control.Write(level, ms);
        return control.Read(new MemoryStream(ms.ToArray()));
    }

    public static TR4Level WriteReadTempLevel(TR4Level level)
    {
        using MemoryStream ms = new();
        TR4LevelControl control = new();
        control.Write(level, ms);
        return control.Read(new MemoryStream(ms.ToArray()));
    }

    public static TR5Level WriteReadTempLevel(TR5Level level)
    {
        using MemoryStream ms = new();
        TR5LevelControl control = new();
        control.Write(level, ms);
        return control.Read(new MemoryStream(ms.ToArray()));
    }

    public static void ReadWritePDP(string levelName, TRGameVersion version)
    {
        levelName = Path.GetFileNameWithoutExtension(levelName) + ".PDP";
        string pathI = GetReadPath(levelName, version, true);

        using FileStream dataStream = File.OpenRead(pathI);
        using MemoryStream inputStream = new();
        using MemoryStream outputStream = new();

        dataStream.CopyTo(inputStream);
        byte[] inputData = inputStream.ToArray();
        inputStream.Position = 0;

        ObserverBase observer;
        switch (version)
        {
            case TRGameVersion.TR1:
                observer = new TR1Observer();
                TR1PDPControl control1 = new(observer);
                TRDictionary<TR1Type, TRModel> models1 = control1.Read(inputStream);
                control1.Write(models1, outputStream);
                break;

            case TRGameVersion.TR2:
                observer = new TR2Observer();
                TR2PDPControl control2 = new(observer);
                TRDictionary<TR2Type, TRModel> models2 = control2.Read(inputStream);
                control2.Write(models2, outputStream);
                break;

            case TRGameVersion.TR3:
                observer = new TR3Observer();
                TR3PDPControl control3 = new(observer);
                TRDictionary<TR3Type, TRModel> models3 = control3.Read(inputStream);
                control3.Write(models3, outputStream);
                break;

            case TRGameVersion.TR4:
                observer = new TR4Observer(true);
                TR4PDPControl control4 = new(observer);
                TRDictionary<TR4Type, TRModel> models4 = control4.Read(inputStream);
                control4.Write(models4, outputStream);
                break;

            case TRGameVersion.TR5:
                observer = new TR5Observer(true);
                TR5PDPControl control5 = new(observer);
                TRDictionary<TR5Type, TRModel> models5 = control5.Read(inputStream);
                control5.Write(models5, outputStream);
                break;

            default:
                throw new NotImplementedException();
        }

        observer.TestOutput(inputData, outputStream.ToArray());
    }

    public static void ReadWriteMAP(string levelName, TRGameVersion version)
    {
        levelName = Path.GetFileNameWithoutExtension(levelName) + ".MAP";
        string pathI = GetReadPath(levelName, version, true);

        using FileStream dataStream = File.OpenRead(pathI);
        using MemoryStream inputStream = new();
        using MemoryStream outputStream = new();

        dataStream.CopyTo(inputStream);
        byte[] inputData = inputStream.ToArray();
        inputStream.Position = 0;

        using StreamReader reader = new(inputStream);
        using StreamWriter writer = new(outputStream);

        switch (version)
        {
            case TRGameVersion.TR1:
                TR1MapControl control1 = new();
                Dictionary<TR1Type, TR1RAlias> map1 = control1.Read(reader);
                control1.Write(map1, writer);
                break;

            case TRGameVersion.TR2:
                TR2MapControl control2 = new();
                Dictionary<TR2Type, TR2RAlias> map2 = control2.Read(reader);
                control2.Write(map2, writer);
                break;

            case TRGameVersion.TR3:
                TR3MapControl control3 = new();
                Dictionary<TR3Type, TR3RAlias> map3 = control3.Read(reader);
                control3.Write(map3, writer);
                break;

            default:
                throw new NotImplementedException();
        }

        // Some maps contain duplicate entries which we eliminate, so just check everything we've
        // written is in the original. Some files also don't end with \r\n, but ours always will,
        // so strip out empty lines for comparison.
        string[] originalLines = Encoding.Default.GetString(inputData).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        string[] outputLines = Encoding.Default.GetString(outputStream.ToArray()).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        CollectionAssert.AreEqual(originalLines.Distinct().ToList(), outputLines);
    }

    public static void ReadWriteTRG(string levelName, TRGameVersion version)
    {
        levelName = Path.GetFileNameWithoutExtension(levelName) + ".TRG";
        string pathI = GetReadPath(levelName, version, true);

        using FileStream dataStream = File.OpenRead(pathI);
        using MemoryStream inputStream = new();
        using MemoryStream outputStream = new();

        dataStream.CopyTo(inputStream);
        byte[] inputData = inputStream.ToArray();
        inputStream.Position = 0;

        ObserverBase observer = new();
        TRGData data = TRGControlBase.Read(inputStream);
        TRGControlBase.Write(data, outputStream);

        observer.TestOutput(inputData, outputStream.ToArray());
    }

    public static IEnumerable<object[]> GetLevelNames(IEnumerable<string> names)
    {
        foreach (string lvl in names)
        {
            yield return new object[] { lvl };
        }
    }
}
