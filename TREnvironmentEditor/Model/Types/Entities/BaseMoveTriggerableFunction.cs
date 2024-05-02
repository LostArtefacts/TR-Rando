using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public abstract class BaseMoveTriggerableFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public EMLocation Location { get; set; }
    public List<EMLocation> TriggerLocations { get; set; }

    protected void RepositionTriggerable(TR1Entity entity, TR1Level level)
    {
        EMLevelData data = GetData(level);

        entity.X = Location.X;
        entity.Y = Location.Y;
        entity.Z = Location.Z;
        entity.Room = data.ConvertRoom(Location.Room);

        if (TriggerLocations == null || TriggerLocations.Count == 0)
        {
            // We want to keep the original triggers
            return;
        }

        // Make a new Trigger based on the first one we find (to ensure things like one-shot are copied)
        // then copy only the action list items for this entity. But if there is already another trigger
        // on the tile, just manually copy over one-shot when appending the new action item.

        List<FDTriggerEntry> currentTriggers = level.FloorData.GetEntityTriggers(EntityIndex);
        level.FloorData.RemoveEntityTriggers(level.Rooms.SelectMany(r => r.Sectors), EntityIndex);

        AmendTriggers(currentTriggers, level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    protected void RepositionTriggerable(TR2Entity entity, TR2Level level)
    {
        EMLevelData data = GetData(level);

        entity.X = Location.X;
        entity.Y = Location.Y;
        entity.Z = Location.Z;
        entity.Room = data.ConvertRoom(Location.Room);

        if (TriggerLocations == null || TriggerLocations.Count == 0)
        {
            return;
        }

        List<FDTriggerEntry> currentTriggers = level.FloorData.GetEntityTriggers(EntityIndex);
        level.FloorData.RemoveEntityTriggers(level.Rooms.SelectMany(r => r.Sectors), EntityIndex);

        AmendTriggers(currentTriggers, level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    protected void RepositionTriggerable(TR3Entity entity, TR3Level level)
    {
        EMLevelData data = GetData(level);

        entity.X = Location.X;
        entity.Y = Location.Y;
        entity.Z = Location.Z;
        entity.Room = data.ConvertRoom(Location.Room);

        List<FDTriggerEntry> currentTriggers = level.FloorData.GetEntityTriggers(EntityIndex);
        level.FloorData.RemoveEntityTriggers(level.Rooms.SelectMany(r => r.Sectors), EntityIndex);

        AmendTriggers(currentTriggers, level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    private void AmendTriggers(List<FDTriggerEntry> currentTriggers, FDControl control, Func<EMLocation, TRRoomSector> sectorGetter)
    {
        foreach (EMLocation location in TriggerLocations)
        {
            TRRoomSector sector = sectorGetter.Invoke(location);
            // If there is no floor data create the FD to begin with.
            if (sector.FDIndex == 0)
            {
                control.CreateFloorData(sector);
            }

            FDActionItem currentObjectAction = null;
            FDTriggerEntry currentTrigger = control[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (currentTrigger != null)
            {
                currentObjectAction = currentTrigger.Actions.Find(a => a.Action == FDTrigAction.Object);
            }

            FDActionItem newAction = new()
            {
                Parameter = (short)EntityIndex
            };

            if (currentObjectAction != null)
            {
                currentTrigger.Actions.Add(newAction);
                if (currentTriggers[0].OneShot)
                {
                    currentTrigger.OneShot = true;
                }
            }
            else
            {
                control[sector.FDIndex].Add(new FDTriggerEntry
                {
                    Mask = currentTriggers[0].Mask,
                    OneShot = currentTriggers[0].OneShot,
                    SwitchOrKeyRef = currentTriggers[0].SwitchOrKeyRef,
                    Timer = currentTriggers[0].Timer,
                    TrigType = currentTriggers[0].TrigType,
                    Actions = new() { newAction }
                });
            }
        }
    }
}
