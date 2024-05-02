using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public static class LocationUtilities
{
    public static Location GetLocation<T>(this TREntity<T> entity)
        where T : Enum
    {
        return new()
        {
            X = entity.X,
            Y = entity.Y,
            Z = entity.Z,
            Room = entity.Room,
            Angle = entity.Angle,
        };
    }

    public static void SetLocation<T>(this TREntity<T> entity, Location location)
        where T : Enum
    {
        entity.X = location.X;
        entity.Y = location.Y;
        entity.Z = location.Z;
        entity.Room = (short)location.Room;
        entity.Angle = location.Angle;
    }

    public static Location GetFloorLocation<T>(this TREntity<T> entity, Func<Location, TRRoomSector> sectorFunc)
        where T : Enum
    {
        return entity.GetLocation().GetFloorLocation(sectorFunc);
    }

    public static Location GetFloorLocation(this Location baseLocation, Func<Location, TRRoomSector> sectorFunc)
    {
        Location location = new()
        {
            X = baseLocation.X,
            Y = baseLocation.Y,
            Z = baseLocation.Z,
            Room = baseLocation.Room,
        };

        HashSet<byte> traversedRooms = new();
        TRRoomSector sector = sectorFunc(location);
        while (sector.RoomBelow != TRConsts.NoRoom && traversedRooms.Add(sector.RoomBelow))
        {
            location.Y = (sector.Floor + 1) * TRConsts.Step1;
            location.Room = sector.RoomBelow;
            sector = sectorFunc(location);
        }

        location.Y = sector.Floor * TRConsts.Step1;
        return location;
    }

    public static bool ContainsSecret(this Location location, TR1Level level)
    {
        TRRoomSector sector = level.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room);
        return SectorContainsSecret(sector, level.FloorData);
    }

    public static bool IsSlipperySlope(this Location location, TR1Level level)
    {
        TRRoomSector sector = level.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room);
        return SectorIsSlipperySlope(sector, level.FloorData);
    }

    public static bool SectorContainsSecret(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex != 0)
        {
            return floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType == FDTrigType.Pickup
                && trigger.Actions.Find(a => a.Action == FDTrigAction.SecretFound) != null;
        }

        return false;
    }

    public static bool HasPickupTriger(TR1Entity entity, int entityIndex, TR1Level level)
    {
        Location floor = entity.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasPickupTrigger(sector, entityIndex, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasPickupTrigger(sector, entityIndex, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasAnyTrigger(Location location, TR1Level level)
    {
        Location floor = location.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasAnyTrigger(sector, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasAnyTrigger(sector, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasPickupTriger(TR2Entity entity, int entityIndex, TR2Level level)
    {
        Location floor = entity.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasPickupTrigger(sector, entityIndex, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasPickupTrigger(sector, entityIndex, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasAnyTrigger(Location location, TR2Level level)
    {
        Location floor = location.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasAnyTrigger(sector, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasAnyTrigger(sector, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasPickupTriger(TR3Entity entity, int entityIndex, TR3Level level)
    {
        Location floor = entity.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasPickupTrigger(sector, entityIndex, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasPickupTrigger(sector, entityIndex, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasAnyTrigger(Location location, TR3Level level)
    {
        Location floor = location.GetFloorLocation(loc =>
            level.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room));
        TRRoomSector sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, (short)floor.Room);
        bool hasTrigger = HasAnyTrigger(sector, level.FloorData);
        if (level.Rooms[floor.Room].AlternateRoom != -1)
        {
            sector = level.GetRoomSector(floor.X, floor.Y, floor.Z, level.Rooms[floor.Room].AlternateRoom);
            hasTrigger |= HasAnyTrigger(sector, level.FloorData);
        }
        return hasTrigger;
    }

    public static bool HasPickupTrigger(TRRoomSector sector, int entityIndex, FDControl floorData)
    {
        return sector.FDIndex != 0
            && floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
            && trigger.TrigType == FDTrigType.Pickup
            && trigger.Actions[0].Parameter == entityIndex;
    }

    public static bool HasAnyTrigger(TRRoomSector sector, FDControl floorData)
    {
        return sector.FDIndex != 0
            && floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger;
    }

    public static bool SectorIsSlipperySlope(TRRoomSector sector, FDControl floorData)
    {
        return sector.FDIndex != 0
            && floorData[sector.FDIndex].Find(e => e is FDSlantEntry slant && slant.Type == FDSlantType.Floor) is FDSlantEntry floorSlant
            && (Math.Abs(floorSlant.XSlant) > 2 || Math.Abs(floorSlant.ZSlant) > 2);
    }

    public static int GetCornerHeight(TRRoomSector sector, FDControl floorData, int x, int z)
    {
        sbyte floor = sector.Floor;
        if (sector.FDIndex != 0)
        {
            FDEntry entry = floorData[sector.FDIndex].Find(e => (e is FDSlantEntry s && s.Type == FDSlantType.Floor)
                || (e is FDTriangulationEntry tri && tri.IsFloorTriangulation));
            if (entry is FDSlantEntry slant)
            {
                sbyte corner0 = sector.Floor;
                sbyte corner1 = sector.Floor;
                sbyte corner2 = sector.Floor;
                sbyte corner3 = sector.Floor;

                if (slant.XSlant > 0)
                {
                    corner0 += slant.XSlant;
                    corner1 += slant.XSlant;
                }
                else if (slant.XSlant < 0)
                {
                    corner2 -= slant.XSlant;
                    corner3 -= slant.XSlant;
                }

                if (slant.ZSlant > 0)
                {
                    corner0 += slant.ZSlant;
                    corner2 += slant.ZSlant;
                }
                else if (slant.ZSlant < 0)
                {
                    corner1 -= slant.ZSlant;
                    corner3 -= slant.ZSlant;
                }

                if ((x & (TRConsts.Step4 - 1)) < TRConsts.Step2)
                {
                    floor = (z & (TRConsts.Step4 - 1)) < TRConsts.Step2 ? corner0 : corner1;
                }
                else
                {
                    floor = (z & (TRConsts.Step4 - 1)) < TRConsts.Step2 ? corner3 : corner2;
                }
            }
            else if (entry is FDTriangulationEntry triangulation)
            {
                List<byte> triangleCorners = new()
                {
                    triangulation.C00,
                    triangulation.C01,
                    triangulation.C10,
                    triangulation.C11
                };

                int max = triangleCorners.Max();
                List<sbyte> corners = new()
                {
                    (sbyte)(max - triangleCorners[0]),
                    (sbyte)(max - triangleCorners[1]),
                    (sbyte)(max - triangleCorners[2]),
                    (sbyte)(max - triangleCorners[3])
                };

                if ((x & (TRConsts.Step4 - 1)) < TRConsts.Step2)
                {
                    floor += (z & (TRConsts.Step4 - 1)) < TRConsts.Step2 ? corners[0] : corners[1];
                }
                else
                {
                    floor += (z & (TRConsts.Step4 - 1)) < TRConsts.Step2 ? corners[2] : corners[3];
                }
            }
        }

        return floor * TRConsts.Step1;
    }
}
