using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
{
    public static class LocationUtilities
    {
        public static bool ContainsSecret(this Location location, TRLevel level, FDControl floorData)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, floorData);
            return SectorContainsSecret(sector, floorData);
        }

        public static bool SectorContainsSecret(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex != 0)
            {
                return floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                    && trigger.TrigType == FDTrigType.Pickup
                    && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null;
            }

            return false;
        }
    }
}