using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl;

public abstract class TRLevelControlBase<L>
    where L : TRLevelBase
{
    protected L _level;

    public L Read(string filePath)
        => Read(File.OpenRead(filePath));

    public L Read(Stream stream)
    {
        using BinaryReader reader = new(stream);

        _level = CreateLevel((TRFileVersion)reader.ReadUInt32());
        Read(reader);

        Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        return _level;
    }

    public void WriteLevelToFile(L level, string filePath)
        => Write(level, File.Create(filePath));

    public void Write(L level, Stream outputStream)
    {
        using BinaryWriter writer = new(outputStream);

        _level = level;
        Write(writer);
    }

    protected abstract L CreateLevel(TRFileVersion version);
    protected abstract void Read(BinaryReader reader);
    protected abstract void Write(BinaryWriter writer);

    protected void TestVersion(L level, params TRFileVersion[] acceptedVersions)
    {
        if (!acceptedVersions.Contains(level.Version.File))
        {
            throw new NotSupportedException($"Unexpected level version: {level.Version.File} ({(uint)level.Version.File})");
        }
    }
}
