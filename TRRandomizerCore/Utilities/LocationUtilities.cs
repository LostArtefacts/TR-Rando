using System.Numerics;
using System;
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

        public static bool IsSlipperySlope(this Location location, TRLevel level, FDControl floorData)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, floorData);
            return SectorIsSlipperySlope(sector, floorData);
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

        public static bool SectorIsSlipperySlope(TRRoomSector sector, FDControl floorData)
        {
            return sector.FDIndex != 0
                && floorData.Entries[sector.FDIndex].Find(e => e is FDSlantEntry slant && slant.Type == FDSlantEntryType.FloorSlant) is FDSlantEntry floorSlant
                && (Math.Abs(floorSlant.XSlant) > 2 || Math.Abs(floorSlant.ZSlant) > 2);
        }
    }
}