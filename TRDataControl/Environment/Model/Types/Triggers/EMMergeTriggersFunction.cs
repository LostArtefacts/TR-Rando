using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMergeTriggersFunction : BaseEMFunction
{
    public EMLocation BaseLocation { get; set; }
    public EMLocation TargetLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.GetRoomSector(data.ConvertLocation(BaseLocation)),
            level.GetRoomSector(data.ConvertLocation(TargetLocation)),
            level.FloorData);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.GetRoomSector(data.ConvertLocation(BaseLocation)),
            level.GetRoomSector(data.ConvertLocation(TargetLocation)),
            level.FloorData);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.GetRoomSector(data.ConvertLocation(BaseLocation)),
            level.GetRoomSector(data.ConvertLocation(TargetLocation)),
            level.FloorData);
    }

    private static void MergeTriggers(TRRoomSector baseSector, TRRoomSector targetSector, FDControl floorData)
    {
        FDEntry baseEntry;
        if (baseSector.FDIndex == 0
            || baseSector == targetSector
            || (baseEntry = floorData[baseSector.FDIndex].Find(e => e is FDTriggerEntry)) == null)
        {
            return;
        }

        if (targetSector.FDIndex == 0)
        {
            floorData.CreateFloorData(targetSector);
        }

        FDEntry targetEntry = floorData[targetSector.FDIndex].Find(e => e is FDTriggerEntry);
        if (targetEntry == null)
        {
            floorData[targetSector.FDIndex].Add(baseEntry);
        }
        else
        {
            FDTriggerEntry baseTrigger = baseEntry as FDTriggerEntry;
            FDTriggerEntry targetTrigger = targetEntry as FDTriggerEntry;

            targetTrigger.Actions.AddRange(baseTrigger.Actions);
            if (baseTrigger.OneShot)
            {
                targetTrigger.OneShot = true;
            }
        }

        floorData[baseSector.FDIndex].Remove(baseEntry);
    }
}
