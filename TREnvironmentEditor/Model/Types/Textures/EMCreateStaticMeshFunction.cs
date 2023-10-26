using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public class EMCreateStaticMeshFunction : BaseEMFunction
{
    public TRMesh Mesh { get; set; }
    public TRStaticMesh Info { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<TRStaticMesh> statics = level.StaticMeshes.ToList();

        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        statics.Add(newMesh);

        level.StaticMeshes = statics.ToArray();
        level.NumStaticMeshes++;
    }

    public override void ApplyToLevel(TR2Level level)
    {
        List<TRStaticMesh> statics = level.StaticMeshes.ToList();

        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        statics.Add(newMesh);

        level.StaticMeshes = statics.ToArray();
        level.NumStaticMeshes++;
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<TRStaticMesh> statics = level.StaticMeshes.ToList();

        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        statics.Add(newMesh);

        level.StaticMeshes = statics.ToArray();
        level.NumStaticMeshes++;
    }
}
