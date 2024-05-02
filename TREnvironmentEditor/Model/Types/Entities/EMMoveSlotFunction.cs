using TREnvironmentEditor.Helpers;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMoveSlotFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public EMLocation Location { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        EntityIndex = data.ConvertEntity(EntityIndex);

        TR1Entity slot = level.Entities[EntityIndex];
        TRRoomSector currentSector = level.GetRoomSector(slot);
        EMLocation location = data.ConvertLocation(Location);
        TRRoomSector newSector = level.GetRoomSector(location);

        // Check if there is also a trigger in the flip map if we are moving the slot within the same room
        TRRoomSector currentFlipSector = null;
        TRRoomSector newFlipSector = null;
        short altRoom = level.Rooms[slot.Room].AlternateRoom;
        if (slot.Room == location.Room && altRoom != -1)
        {
            currentFlipSector = level.GetRoomSector(slot.X, slot.Y, slot.Z, altRoom);
            newFlipSector = level.GetRoomSector(Location.X, Location.Y, Location.Z, altRoom);
        }

        MoveSlot(level.FloorData, slot, location, currentSector, newSector, currentFlipSector, newFlipSector);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        EntityIndex = data.ConvertEntity(EntityIndex);

        TR2Entity slot = level.Entities[EntityIndex];
        TRRoomSector currentSector = level.GetRoomSector(slot);
        EMLocation location = data.ConvertLocation(Location);
        TRRoomSector newSector = level.GetRoomSector(location);

        TRRoomSector currentFlipSector = null;
        TRRoomSector newFlipSector = null;
        short altRoom = level.Rooms[slot.Room].AlternateRoom;
        if (slot.Room == location.Room && altRoom != -1)
        {
            currentFlipSector = level.GetRoomSector(slot.X, slot.Y, slot.Z, altRoom);
            newFlipSector = level.GetRoomSector(Location.X, Location.Y, Location.Z, altRoom);
        }

        // Make sure there isn't a static enemy on the same sector e.g. MorayEel
        List<TR2Entity> staticEnemies = level.Entities.FindAll(e => e.Room == location.Room && TR2TypeUtilities.IsStaticCreature(e.TypeID));
        foreach (TR2Entity staticEnemy in staticEnemies)
        {
            TRRoomSector enemySector = level.GetRoomSector(staticEnemy);
            if (enemySector == newSector)
            {
                return;
            }
        }

        MoveSlot(level.FloorData, slot, location, currentSector, newSector, currentFlipSector, newFlipSector);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        EntityIndex = data.ConvertEntity(EntityIndex);

        TR3Entity slot = level.Entities[EntityIndex];
        TRRoomSector currentSector = level.GetRoomSector(slot);
        EMLocation location = data.ConvertLocation(Location);
        TRRoomSector newSector = level.GetRoomSector(location);

        TRRoomSector currentFlipSector = null;
        TRRoomSector newFlipSector = null;
        short altRoom = level.Rooms[slot.Room].AlternateRoom;
        if (slot.Room == location.Room && altRoom != -1)
        {
            currentFlipSector = level.GetRoomSector(slot.X, slot.Y, slot.Z, altRoom);
            newFlipSector = level.GetRoomSector(Location.X, Location.Y, Location.Z, altRoom);
        }

        MoveSlot(level.FloorData, slot, location, currentSector, newSector, currentFlipSector, newFlipSector);
    }

    protected void MoveSlot<T>(FDControl control, TREntity<T> slot, EMLocation location, 
        TRRoomSector currentSector, TRRoomSector newSector, TRRoomSector currentFlipSector, TRRoomSector newFlipSector)
        where T : Enum
    {
        slot.X = location.X;
        slot.Y = location.Y;
        slot.Z = location.Z;
        slot.Room = location.Room;
        slot.Angle = location.Angle;

        if (newSector != currentSector && currentSector.FDIndex != 0)
        {
            MoveTriggers(control, currentSector, newSector);

            if (currentFlipSector != null && newFlipSector != null && currentFlipSector.FDIndex != 0)
            {
                MoveTriggers(control, currentFlipSector, newFlipSector);
            }
        }
    }

    protected void MoveTriggers(FDControl control, TRRoomSector currentSector, TRRoomSector newSector)
    {
        if (newSector.FDIndex == 0)
        {
            control.CreateFloorData(newSector);
        }

        bool keyTrigPredicate(FDEntry e) => e is FDTriggerEntry trig && trig.SwitchOrKeyRef == EntityIndex;

        // Copy the key trigger to the new sector
        List<FDEntry> keyTriggers = control[currentSector.FDIndex].FindAll(keyTrigPredicate);
        control[newSector.FDIndex].AddRange(keyTriggers);
        // Remove from the old
        control[currentSector.FDIndex].RemoveAll(keyTrigPredicate);
    }
}
