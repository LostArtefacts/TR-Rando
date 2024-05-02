using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMResetPickupTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        ResetPickupTriggers(level.FloorData, l => level.FloorData.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        ResetPickupTriggers(level.FloorData, l => level.FloorData.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        ResetPickupTriggers(level.FloorData, l => level.FloorData.GetRoomSector(l.X, l.Y, l.Z, data.ConvertRoom(l.Room), level));
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

            FDEntry entry = floorData[sector.FDIndex].Find(e => e is FDTriggerEntry);
            if (entry is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.Actions.Count > 1)
            {
                trigger.Actions.RemoveRange(1, trigger.Actions.Count - 1);
            }
        }
    }
}
