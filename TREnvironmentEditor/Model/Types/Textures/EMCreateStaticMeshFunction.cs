using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public class EMCreateStaticMeshFunction : BaseEMFunction
{
    public TRMesh Mesh { get; set; }
    public TRStaticMesh Info { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        level.StaticMeshes.Add(newMesh);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        level.StaticMeshes.Add(newMesh);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = (ushort)TRMeshUtilities.InsertMesh(level, Mesh);
        level.StaticMeshes.Add(newMesh);
    }
}
