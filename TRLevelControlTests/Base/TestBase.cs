using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;
using TRLevelControl.Compression;
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
        TR1LevelReader control = new();
        return control.ReadLevel(GetReadPath(level, TRGameVersion.TR1));
    }

    public static TR2Level GetTR2Level(string level)
    {
        TR2LevelReader control = new();
        return control.ReadLevel(GetReadPath(level, TRGameVersion.TR2));
    }

    public static TR3Level GetTR3Level(string level)
    {
        TR3LevelReader control = new();
        return control.ReadLevel(GetReadPath(level, TRGameVersion.TR3));
    }

    public static TR4Level GetTR4Level(string level)
    {
        TR4LevelReader control = new();
        return control.ReadLevel(GetReadPath(level, TRGameVersion.TR4));
    }

    public static TR5Level GetTR5Level(string level)
    {
        TR5LevelReader control = new();
        return control.ReadLevel(GetReadPath(level, TRGameVersion.TR5));
    }

    public static void ReadWriteLevel(string levelName, TRGameVersion version)
    {
        string pathI = GetReadPath(levelName, version);
        string pathO = GetWritePath(levelName, version);

        switch (version)
        {
            case TRGameVersion.TR1:
                TR1Level level1 = new TR1LevelReader().ReadLevel(pathI);
                new TR1LevelWriter().WriteLevelToFile(level1, pathO);
                break;
            case TRGameVersion.TR2:
                TR2Level level2 = new TR2LevelReader().ReadLevel(pathI);
                new TR2LevelWriter().WriteLevelToFile(level2, pathO);
                break;
            case TRGameVersion.TR3:
                TR3Level level3 = new TR3LevelReader().ReadLevel(pathI);
                new TR3LevelWriter().WriteLevelToFile(level3, pathO);
                break;
            default:
                throw new Exception("Utility IO method suitable only for TR1-3.");
        }

        // TODO: allow level control to read/write from stream and not files alone
        byte[] b1 = File.ReadAllBytes(pathI);
        byte[] b2 = File.ReadAllBytes(pathO);

        CollectionAssert.AreEqual(b1, b2);
    }

    public static void ReadWriteTR4Level(string levelName)
    {
        TR4LevelReader reader = new();
        TR4LevelWriter writer = new();

        string pathI = GetReadPath(levelName, TRGameVersion.TR4);
        string pathO = GetWritePath(levelName, TRGameVersion.TR4);

        // ZLib produces a slightly more optimal output than OG so we can't compare byte-for-byte
        TR4Level level = reader.ReadLevel(pathI);
        TR45LevelSummary originalSummary = new()
        {
            LevelChunkUncompressedSize = level.LevelDataChunk.UncompressedSize,
            Tex32ChunkUncompressedSize = level.Texture32Chunk.UncompressedSize,
            Tex16ChunkUncompressedSize = level.Texture16Chunk.UncompressedSize,
            Tex32MChunkUncompressedSize = level.SkyAndFont32Chunk.UncompressedSize
        };

        writer.WriteLevelToFile(level, pathO);
        // Read in again what we wrote out
        TR4Level level2 = reader.ReadLevel(pathI);

        // Verify - have we lost any data?
        Assert.AreEqual(originalSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(level2.LevelDataChunk.CompressedChunk).Length);
        Assert.AreEqual(originalSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(level2.Texture32Chunk.CompressedChunk).Length);
        Assert.AreEqual(originalSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(level2.Texture16Chunk.CompressedChunk).Length);
        Assert.AreEqual(originalSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(level2.SkyAndFont32Chunk.CompressedChunk).Length);

        // Test compression against original
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, level2.LevelDataChunk.Seperator);
        CollectionAssert.AreEqual(TRZlib.Decompress(level.Texture32Chunk.CompressedChunk), TRZlib.Decompress(level2.Texture32Chunk.CompressedChunk));
        CollectionAssert.AreEqual(TRZlib.Decompress(level.Texture16Chunk.CompressedChunk), TRZlib.Decompress(level2.Texture16Chunk.CompressedChunk));
        CollectionAssert.AreEqual(TRZlib.Decompress(level.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(level2.SkyAndFont32Chunk.CompressedChunk));
    }

    public static void ReadWriteTR5Level(string levelName)
    {
        TR5LevelReader reader = new();
        TR5LevelWriter writer = new();

        string pathI = GetReadPath(levelName, TRGameVersion.TR5);
        string pathO = GetWritePath(levelName, TRGameVersion.TR5);

        // ZLib produces a slightly more optimal output than OG so we can't compare byte-for-byte
        TR5Level level = reader.ReadLevel(pathI);
        TR45LevelSummary originalSummary = new()
        {
            LevelChunkUncompressedSize = level.LevelDataChunk.UncompressedSize,
            Tex32ChunkUncompressedSize = level.Texture32Chunk.UncompressedSize,
            Tex16ChunkUncompressedSize = level.Texture16Chunk.UncompressedSize,
            Tex32MChunkUncompressedSize = level.SkyAndFont32Chunk.UncompressedSize
        };

        writer.WriteLevelToFile(level, pathO);
        // Read in again what we wrote out
        TR5Level level2 = reader.ReadLevel(pathI);

        // Verify - have we lost any data?
        Assert.AreEqual(originalSummary.LevelChunkUncompressedSize, (uint)level2.LevelDataChunk.CompressedChunk.Length);
        Assert.AreEqual(originalSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(level2.Texture32Chunk.CompressedChunk).Length);
        Assert.AreEqual(originalSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(level2.Texture16Chunk.CompressedChunk).Length);
        Assert.AreEqual(originalSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(level2.SkyAndFont32Chunk.CompressedChunk).Length);

        // Test compression against original
        CollectionAssert.AreEqual(new byte[] { 0xCD, 0xCD, 0xCD, 0xCD, 0xCD, 0xCD }, level2.LevelDataChunk.Seperator);
        CollectionAssert.AreEqual(TRZlib.Decompress(level.Texture32Chunk.CompressedChunk), TRZlib.Decompress(level2.Texture32Chunk.CompressedChunk));
        CollectionAssert.AreEqual(TRZlib.Decompress(level.Texture16Chunk.CompressedChunk), TRZlib.Decompress(level2.Texture16Chunk.CompressedChunk));
        CollectionAssert.AreEqual(TRZlib.Decompress(level.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(level2.SkyAndFont32Chunk.CompressedChunk));
    }

    public static TR1Level WriteReadTempLevel(TR1Level level)
        => WriteReadTempLevel(level, "TEST1.PHD");

    public static TR2Level WriteReadTempLevel(TR2Level level)
        => WriteReadTempLevel(level, "TEST1.TR2");

    public static TR3Level WriteReadTempLevel(TR3Level level)
        => WriteReadTempLevel(level, "TEST1.TR2");

    public static TR4Level WriteReadTempLevel(TR4Level level)
        => WriteReadTempLevel(level, "TEST1.TR4");

    public static TR5Level WriteReadTempLevel(TR5Level level)
        => WriteReadTempLevel(level, "TEST1.TRC");

    public static TR1Level WriteReadTempLevel(TR1Level level, string levelName)
    {
        // TODO: allow level control to read/write from stream and not files alone
        string path = GetWritePath(levelName, TRGameVersion.TR1);
        TR1LevelReader reader = new();
        TR1LevelWriter writer = new();
        writer.WriteLevelToFile(level, path);
        return reader.ReadLevel(path);
    }

    public static TR2Level WriteReadTempLevel(TR2Level level, string levelName)
    {
        string path = GetWritePath(levelName, TRGameVersion.TR2);
        TR2LevelReader reader = new();
        TR2LevelWriter writer = new();
        writer.WriteLevelToFile(level, path);
        return reader.ReadLevel(path);
    }

    public static TR3Level WriteReadTempLevel(TR3Level level, string levelName)
    {
        string path = GetWritePath(levelName, TRGameVersion.TR3);
        TR3LevelReader reader = new();
        TR3LevelWriter writer = new();
        writer.WriteLevelToFile(level, path);
        return reader.ReadLevel(path);
    }

    public static TR4Level WriteReadTempLevel(TR4Level level, string levelName)
    {
        string path = GetWritePath(levelName, TRGameVersion.TR4);
        TR4LevelReader reader = new();
        TR4LevelWriter writer = new();
        writer.WriteLevelToFile(level, path);
        return reader.ReadLevel(path);
    }

    public static TR5Level WriteReadTempLevel(TR5Level level, string levelName)
    {
        string path = GetWritePath(levelName, TRGameVersion.TR5);
        TR5LevelReader reader = new();
        TR5LevelWriter writer = new();
        writer.WriteLevelToFile(level, path);
        return reader.ReadLevel(path);
    }
}
