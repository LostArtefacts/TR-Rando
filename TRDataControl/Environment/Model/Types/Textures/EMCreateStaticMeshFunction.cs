using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMCreateStaticMeshFunction : BaseEMFunction
{
    public uint ID { get; set; }
    public TRStaticMesh Info { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        level.StaticMeshes[(TR1Type)ID] = Info.Clone();
    }

    public override void ApplyToLevel(TR2Level level)
    {
        level.StaticMeshes[(TR2Type)ID] = Info.Clone();
    }

    public override void ApplyToLevel(TR3Level level)
    {
        level.StaticMeshes[(TR3Type)ID] = Info.Clone();
    }
}
