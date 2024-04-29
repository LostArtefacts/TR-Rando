using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public class EMCreateStaticMeshFunction : BaseEMFunction
{
    public uint ID { get; set; }
    public TRMesh Mesh { get; set; }
    public TRStaticMesh Info { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = Mesh;
        level.StaticMeshes[(TR1Type)ID] = newMesh;
    }

    public override void ApplyToLevel(TR2Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = Mesh;
        level.StaticMeshes[(TR2Type)ID] = newMesh;
    }

    public override void ApplyToLevel(TR3Level level)
    {
        TRStaticMesh newMesh = Info.Clone();
        newMesh.Mesh = Mesh;
        level.StaticMeshes[(TR3Type)ID] = newMesh;
    }
}
