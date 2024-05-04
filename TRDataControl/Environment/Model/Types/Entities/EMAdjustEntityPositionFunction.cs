using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAdjustEntityPositionFunction : BaseEMFunction
{
    public short EntityType { get; set; }
    public Dictionary<int, Dictionary<short, EMLocation>> RoomMap { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<TR1Entity> entities = level.Entities.FindAll(e => (short)e.TypeID == EntityType);
        AdjustEntities(entities);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        // Example use case is rotating wall blades, which need various different angles across the levels after mirroring.
        // X, Y, Z in the target relocation will be relative to the current location; the angle will be the new angle.

        List<TR2Entity> entities = level.Entities.FindAll(e => (short)e.TypeID == EntityType);
        AdjustEntities(entities);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<TR3Entity> entities = level.Entities.FindAll(e => (short)e.TypeID == EntityType);
        AdjustEntities(entities);
    }

    private void AdjustEntities<T>(IEnumerable<TREntity<T>> entities)
        where T : Enum
    {
        foreach (int roomNumber in RoomMap.Keys)
        {
            foreach (short currentAngle in RoomMap[roomNumber].Keys)
            {
                EMLocation relocation = RoomMap[roomNumber][currentAngle];
                IEnumerable<TREntity<T>> matchingEntities = entities.Where(e => e.Room == roomNumber && e.Angle == currentAngle);
                foreach (TREntity<T> match in matchingEntities)
                {
                    match.X += relocation.X;
                    match.Y += relocation.Y;
                    match.Z += relocation.Z;
                    match.Angle = relocation.Angle;
                }
            }
        }
    }
}
