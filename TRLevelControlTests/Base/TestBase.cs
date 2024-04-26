using TRLevelControl;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class TestBase
{
    protected static readonly string _readPath = @"Levels\{0}\{1}";
    protected static readonly string _writePath = @"Levels\{0}\{1}_TEMP{2}";

    public static string GetReadPath(string level, TRGameVersion version)
    {
        return string.Format(_readPath, version.ToString(), level);
    }

    public static string GetWritePath(string level, TRGameVersion version)
    {
        return string.Format(_writePath, version.ToString(), Path.GetFileNameWithoutExtension(level), Path.GetExtension(level));
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

    public static TR5Level GetTR5Level(string level)
    {
        TR5LevelControl control = new();
        return control.Read(GetReadPath(level, TRGameVersion.TR5));
    }

    public static void ReadWriteLevel(string levelName, TRGameVersion version)
    {
        string pathI = GetReadPath(levelName, version);
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
                observer = new TR3Observer();
                TR3LevelControl control3 = new(observer);
                TR3Level level3 = control3.Read(pathI);
                control3.Write(level3, outputStream);
                break;
            case TRGameVersion.TR4:
                observer = new TR4Observer();
                TR4LevelControl control4 = new(observer);
                TR4Level level4 = control4.Read(pathI);
                control4.Write(level4, outputStream);
                break;
            case TRGameVersion.TR5:
                observer = new TR5Observer();
                TR5LevelControl control5 = new(observer);
                TR5Level level5 = control5.Read(pathI);
                control5.Write(level5, outputStream);
                break;
            default:
                throw new NotImplementedException();
        }

        observer.TestOutput(inputData, outputStream.ToArray());
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
}
