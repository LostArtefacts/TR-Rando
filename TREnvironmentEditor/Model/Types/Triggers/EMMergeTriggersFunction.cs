using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMergeTriggersFunction : BaseEMFunction
{
    public EMLocation BaseLocation { get; set; }
    public EMLocation TargetLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level),
            level.FloorData.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level),
            level.FloorData);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level),
            level.FloorData.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level),
            level.FloorData);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        MergeTriggers(level.FloorData.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level),
            level.FloorData.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level),
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
