using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMergeTriggersFunction : BaseEMFunction
{
    public EMLocation BaseLocation { get; set; }
    public EMLocation TargetLocation { get; set; }
    public List<EMLocation> TargetLocations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        var data = GetData(level);
        MergeTriggers(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        var data = GetData(level);
        MergeTriggers(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        var data = GetData(level);
        MergeTriggers(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));
    }

    private void MergeTriggers(FDControl floorData, Func<EMLocation, TRRoomSector> getSector)
    {
        var baseSector = getSector(BaseLocation);
        if (baseSector.FDIndex == 0
            || floorData[baseSector.FDIndex].OfType<FDTriggerEntry>().FirstOrDefault() is not FDTriggerEntry baseTrigger)
        {
            return;
        }

        var targets = new List<EMLocation>();
        if (TargetLocations != null)
        {
            targets.AddRange(TargetLocations);
        }
        if (TargetLocation != null)
        {
            targets.Add(TargetLocation); // Legacy
        }

        foreach (var location in targets)
        {
            var targetSector = getSector(location);
            if (baseSector == targetSector)
            {
                continue;
            }

            if (targetSector.FDIndex == 0)
            {
                floorData.CreateFloorData(targetSector);
            }

            if (floorData[targetSector.FDIndex].
                OfType<FDTriggerEntry>().FirstOrDefault() is not FDTriggerEntry targetTrigger)
            {
                floorData[targetSector.FDIndex].Add(baseTrigger);
            }
            else
            {
                targetTrigger.Actions.AddRange(baseTrigger.Actions);
                targetTrigger.OneShot |= baseTrigger.OneShot;
            }
        }

        floorData[baseSector.FDIndex].Remove(baseTrigger);
    }
}
