using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public abstract class TRPDPControlBase<T>
    where T : Enum
{
    private static readonly DummyMeshProvider _meshProvider = new();

    protected ITRLevelObserver _observer;

    public TRPDPControlBase(ITRLevelObserver observer = null)
    {
        _observer = observer;
    }

    public TRDictionary<T, TRModel> Read(string filePath)
        => Read(File.OpenRead(filePath));

    public TRDictionary<T, TRModel> Read(Stream stream)
    {
        using TRLevelReader reader = new(stream);

        TRModelBuilder<T> builder = CreateBuilder();
        TRDictionary<T, TRModel> models = builder.ReadModelData(reader, _meshProvider);

        Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);
        return models;
    }

    public void Write(TRDictionary<T, TRModel> models, string filePath)
        => Write(models, File.Create(filePath));

    public void Write(TRDictionary<T, TRModel> models, Stream outputStream)
    {
        using TRLevelWriter writer = new(outputStream);

        TRModelBuilder<T> builder = CreateBuilder();
        builder.WriteModelData(writer, models);
    }

    protected abstract TRModelBuilder<T> CreateBuilder();
}
