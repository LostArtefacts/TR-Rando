using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRModelBuilder
{
    private static readonly ushort _tr5ModelPadding = 0xFFEF;

    private readonly TRGameVersion _version;

    public TRModelBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public List<TRModel> ReadModels(TRLevelReader reader)
    {
        uint numModels = reader.ReadUInt32();
        List<TRModel> models = new();

        for (int i = 0; i < numModels; i++)
        {
            TRModel model = new()
            {
                ID = reader.ReadUInt32(),
                NumMeshes = reader.ReadUInt16(),
                StartingMesh = reader.ReadUInt16(),
                MeshTree = reader.ReadUInt32(),
                FrameOffset = reader.ReadUInt32(),
                Animation = reader.ReadUInt16()
            };

            if (_version == TRGameVersion.TR5)
            {
                Debug.Assert(reader.ReadUInt16() == _tr5ModelPadding);
            }

            models.Add(model);
        }

        return models;
    }

    public void WriteModels(List<TRModel> models, TRLevelWriter writer)
    {
        writer.Write((uint)models.Count);

        foreach (TRModel model in models)
        {
            writer.Write(model.ID);
            writer.Write(model.NumMeshes);
            writer.Write(model.StartingMesh);
            writer.Write(model.MeshTree);
            writer.Write(model.FrameOffset);
            writer.Write(model.Animation);

            if (_version == TRGameVersion.TR5)
            {
                writer.Write(_tr5ModelPadding);
            }
        }
    }
}
