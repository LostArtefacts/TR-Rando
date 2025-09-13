using TRLevelControl.Model;

namespace TRDataControl.Environment;

public abstract class BaseMoveTriggerableFunction : BaseEMFunction
{
    public int EntityIndex { get; set; }
    public EMLocation Location { get; set; }
    public List<EMLocation> TriggerLocations { get; set; }

    protected void RepositionTriggerable(TR1Entity entity, TR1Level level)
    {
        var data = GetData(level);
        RepositionTriggerable(entity, level.Entities.IndexOf(entity), data,
            level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    protected void RepositionTriggerable(TR2Entity entity, TR2Level level)
    {
        var data = GetData(level);
        RepositionTriggerable(entity, level.Entities.IndexOf(entity), data,
            level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    protected void RepositionTriggerable(TR3Entity entity, TR3Level level)
    {
        var data = GetData(level);
        RepositionTriggerable(entity, level.Entities.IndexOf(entity), data,
            level.FloorData, location => level.GetRoomSector(data.ConvertLocation(location)));
    }

    private void RepositionTriggerable(ITRLocatable entity, int entityIdx, EMLevelData data,
        FDControl floorData, Func<EMLocation, TRRoomSector> sectorGetter)
    {
        entity.X = Location.X;
        entity.Y = Location.Y;
        entity.Z = Location.Z;
        entity.Room = data.ConvertRoom(Location.Room);

        var currentTriggers = floorData.GetEntityTriggers(entityIdx);
        if (TriggerLocations == null || TriggerLocations.Count == 0
            || currentTriggers.Count == 0)
        {
            return;
        }

        var baseTrigger = currentTriggers[0];
        floorData.RemoveEntityTriggers(entityIdx);
        
        foreach (var location in TriggerLocations)
        {
            var sector = sectorGetter(location);
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            var sectorFD = floorData[sector.FDIndex];
            if (sectorFD.Find(e => e is FDTriggerEntry) is not FDTriggerEntry targetTrigger)
            {
                targetTrigger = new()
                {
                    Mask = baseTrigger.Mask,
                    SwitchOrKeyRef = baseTrigger.SwitchOrKeyRef,
                    Timer = baseTrigger.Timer,
                    TrigType = baseTrigger.TrigType,
                };
                sectorFD.Add(targetTrigger);
            }

            targetTrigger.OneShot |= baseTrigger.OneShot;
            targetTrigger.Actions.Add(new()
            {
                Action = FDTrigAction.Object,
                Parameter = (short)entityIdx,
            });
        }
    }
}
