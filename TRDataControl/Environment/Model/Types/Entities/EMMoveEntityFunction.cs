using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMoveEntityFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public EMLocation TargetLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        MoveEntity(level.Entities[data.ConvertEntity(EntityIndex)], data);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        MoveEntity(level.Entities[data.ConvertEntity(EntityIndex)], data);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        MoveEntity(level.Entities[data.ConvertEntity(EntityIndex)], data);
    }

    private void MoveEntity<T>(TREntity<T> entity, EMLevelData data)
        where T : Enum
    {
        entity.X = TargetLocation.X;
        entity.Y = TargetLocation.Y;
        entity.Z = TargetLocation.Z;
        entity.Room = data.ConvertRoom(TargetLocation.Room);
        entity.Angle = TargetLocation.Angle;
    }
}
