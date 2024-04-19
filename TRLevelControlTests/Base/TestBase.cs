using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        using MemoryStream outputStream = new();

        switch (version)
        {
            case TRGameVersion.TR1:
                TR1LevelControl control1 = new();
                TR1Level level1 = control1.Read(pathI);
                control1.Write(level1, outputStream);
                break;
            case TRGameVersion.TR2:
                TR2LevelControl control2 = new();
                TR2Level level2 = control2.Read(pathI);
                control2.Write(level2, outputStream);
                break;
            case TRGameVersion.TR3:
                TR3LevelControl control3 = new();
                TR3Level level3 = control3.Read(pathI);
                control3.Write(level3, outputStream);
                break;
            default:
                throw new Exception("Utility IO method suitable only for TR1-3.");
        }

        byte[] b1 = File.ReadAllBytes(pathI);
        byte[] b2 = outputStream.ToArray();

        CollectionAssert.AreEqual(b1, b2);
    }

    public static void ReadWriteTR4Level(string levelName)
    {
        TR4LevelControl control = new();

        string pathI = GetReadPath(levelName, TRGameVersion.TR4);
        using MemoryStream outputStream1 = new();
        using MemoryStream outputStream2 = new();

        TR4Level level = control.Read(pathI);
        control.Write(level, outputStream1);

        byte[] output1 = outputStream1.ToArray();

        // Read in again what we wrote out
        TR4Level level2 = control.Read(new MemoryStream(output1));
        control.Write(level2, outputStream2);

        byte[] output2 = outputStream2.ToArray();

        CollectionAssert.AreEqual(output1, output2);
    }

    public static void ReadWriteTR5Level(string levelName)
    {
        TR5LevelControl control = new();

        string pathI = GetReadPath(levelName, TRGameVersion.TR5);
        using MemoryStream outputStream1 = new();
        using MemoryStream outputStream2 = new();

        TR5Level level = control.Read(pathI);
        control.Write(level, outputStream1);

        byte[] output1 = outputStream1.ToArray();

        // Read in again what we wrote out
        TR5Level level2 = control.Read(new MemoryStream(output1));
        control.Write(level2, outputStream2);

        byte[] output2 = outputStream2.ToArray();

        CollectionAssert.AreEqual(output1, output2);
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
