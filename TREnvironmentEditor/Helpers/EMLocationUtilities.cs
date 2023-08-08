using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;

namespace TREnvironmentEditor.Helpers;

public static class EMLocationUtilities
{
    public static int GetContainedSecretEntity(this EMLocation location, TR1Level level, FDControl floorData)
    {
        TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
        return GetSectorSecretEntity(sector, floorData);
    }

    public static int GetContainedSecretEntity(this EMLocation location, TR2Level level, FDControl floorData)
    {
        TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
        for (int i = 0; i < level.NumEntities; i++)
        {
            TR2Entity entity = level.Entities[i];
            if (TR2EntityUtilities.IsSecretType((TR2Entities)entity.TypeID)
                && FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData) == sector)
            {
                return i;
            }
        }
        return -1;
    }

    public static int GetContainedSecretEntity(this EMLocation location, TR3Level level, FDControl floorData)
    {
        TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
        return GetSectorSecretEntity(sector, floorData);
    }

    private static int GetSectorSecretEntity(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex != 0)
        {
            if (floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null
                && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.Object) is FDActionListItem action)
            {
                return action.Parameter;
            }
        }

        return -1;
    }
}
