using System.Diagnostics;
using TRLevelControl.IO;
using TRLevelControl.Model;

namespace TRLevelControl;

public abstract class TRLevelControlBase<L>
    where L : TRLevelBase
{
    protected ITRLevelObserver _observer;
    protected L _level;

    public TRLevelControlBase(ITRLevelObserver observer = null)
    {
        _observer = observer;
    }

    public L Read(string filePath)
        => Read(File.OpenRead(filePath));

    public L Read(Stream stream)
    {
        using TRLevelReader reader = new(stream, _observer);

        _level = CreateLevel((TRFileVersion)reader.ReadUInt32());
        Initialise();
        Read(reader);
        _level.TRXData = TRXInjector.Read(reader);

        Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        return _level;
    }

    public void Write(L level, string filePath)
        => Write(level, File.Create(filePath));

    public void Write(L level, Stream outputStream)
    {
        using TRLevelWriter writer = new(outputStream, _observer);

        writer.Write((uint)level.Version.File);

        _level = level;
        Initialise();
        Write(writer);
        TRXInjector.Write(level.TRXData, writer);
    }

    protected abstract L CreateLevel(TRFileVersion version);
    protected abstract void Initialise();
    protected abstract void Read(TRLevelReader reader);
    protected abstract void Write(TRLevelWriter writer);

    protected void TestVersion(L level, params TRFileVersion[] acceptedVersions)
    {
        if (!acceptedVersions.Contains(level.Version.File))
        {
            throw new NotSupportedException($"Unexpected level version: {level.Version.File} ({(uint)level.Version.File})");
        }
    }
}
