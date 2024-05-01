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
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        MergeTriggers(
            FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, floorData),
            FDUtilities.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level, floorData),
            floorData);

        floorData.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        MergeTriggers(
            FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, floorData),
            FDUtilities.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level, floorData),
            floorData);

        floorData.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        MergeTriggers(
            FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, floorData),
            FDUtilities.GetRoomSector(TargetLocation.X, TargetLocation.Y, TargetLocation.Z, data.ConvertRoom(TargetLocation.Room), level, floorData),
            floorData);

        floorData.WriteToLevel(level);
    }

    private static void MergeTriggers(TRRoomSector baseSector, TRRoomSector targetSector, FDControl floorData)
    {
        FDEntry baseEntry;
        if (baseSector.FDIndex == 0
            || baseSector == targetSector
            || (baseEntry = floorData.Entries[baseSector.FDIndex].Find(e => e is FDTriggerEntry)) == null)
        {
            return;
        }

        if (targetSector.FDIndex == 0)
        {
            floorData.CreateFloorData(targetSector);
        }

        FDEntry targetEntry = floorData.Entries[targetSector.FDIndex].Find(e => e is FDTriggerEntry);
        if (targetEntry == null)
        {
            floorData.Entries[targetSector.FDIndex].Add(baseEntry);
        }
        else
        {
            FDTriggerEntry baseTrigger = baseEntry as FDTriggerEntry;
            FDTriggerEntry targetTrigger = targetEntry as FDTriggerEntry;

            targetTrigger.TrigActionList.AddRange(baseTrigger.TrigActionList);
            if (baseTrigger.TrigSetup.OneShot)
            {
                targetTrigger.TrigSetup.OneShot = true;
            }
        }

        floorData.Entries[baseSector.FDIndex].Remove(baseEntry);
        if (floorData.Entries[baseSector.FDIndex].Count == 0)
        {
            floorData.RemoveFloorData(baseSector);
        }
    }
}
