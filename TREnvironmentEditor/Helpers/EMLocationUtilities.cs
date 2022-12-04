using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Helpers
{
    public static class EMLocationUtilities
    {
        public static int GetContainedSecret(this EMLocation location, TRLevel level, FDControl floorData)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
            return GetSectorSecret(sector, floorData);
        }

        public static int GetContainedSecret(this EMLocation location, TR2Level level, FDControl floorData)
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

        public static int GetContainedSecret(this EMLocation location, TR3Level level, FDControl floorData)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
            return GetSectorSecret(sector, floorData);
        }

        private static int GetSectorSecret(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex != 0)
            {
                if (floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                    && trigger.TrigType == FDTrigType.Pickup
                    && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null)
                {
                    return trigger.TrigActionList.First().Parameter;
                }
            }

            return -1;
        }
    }
}
