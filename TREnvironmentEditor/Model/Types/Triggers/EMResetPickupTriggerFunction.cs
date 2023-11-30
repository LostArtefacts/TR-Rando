using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMResetPickupTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        ResetPickupTriggers(floorData, l => FDUtilities.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level, floorData));

        floorData.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        ResetPickupTriggers(floorData, l => FDUtilities.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level, floorData));

        floorData.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        ResetPickupTriggers(floorData, l => FDUtilities.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level, floorData));

        floorData.WriteToLevel(level);
    }

    private void ResetPickupTriggers(FDControl floorData, Func<EMLocation, TRRoomSector> sectorFunc)
    {
        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = sectorFunc(location);
            if (sector.FDIndex == 0)
            {
                continue;
            }

            FDEntry entry = floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry);
            if (entry is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.TrigActionList.Count > 1)
            {
                trigger.TrigActionList.RemoveRange(1, trigger.TrigActionList.Count - 1);
            }
        }
    }
}
