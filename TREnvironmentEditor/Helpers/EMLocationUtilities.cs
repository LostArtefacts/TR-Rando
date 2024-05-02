using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

public static class EMLocationUtilities
{
    public static int GetContainedSecretEntity(this EMLocation location, TR1Level level)
    {
        TRRoomSector sector = level.GetRoomSector(location);
        return GetSectorSecretEntity(sector, level.FloorData);
    }

    public static int GetContainedSecretEntity(this EMLocation location, TR2Level level)
    {
        TRRoomSector sector = level.GetRoomSector(location);
        return level.Entities.FindIndex(e => TR2TypeUtilities.IsSecretType(e.TypeID) && level.GetRoomSector(e) == sector);
    }

    public static int GetContainedSecretEntity(this EMLocation location, TR3Level level)
    {
        TRRoomSector sector = level.GetRoomSector(location);
        return GetSectorSecretEntity(sector, level.FloorData);
    }

    private static int GetSectorSecretEntity(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex != 0)
        {
            if (floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.Actions.Find(a => a.Action == FDTrigAction.SecretFound) != null
                && trigger.Actions.Find(a => a.Action == FDTrigAction.Object) is FDActionItem action)
            {
                return action.Parameter;
            }
        }

        return -1;
    }
}
