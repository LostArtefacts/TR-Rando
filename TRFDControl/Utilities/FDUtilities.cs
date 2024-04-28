using TRFDControl.FDEntryTypes;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRFDControl.Utilities;

public static class FDUtilities
{
    public static List<FDTriggerEntry> GetEntityTriggers(FDControl control, int entityIndex)
    {
        return GetTriggers(control, FDTrigAction.Object, entityIndex);
    }

    public static List<FDTriggerEntry> GetSecretTriggers(FDControl control, int secretIndex)
    {
        return GetTriggers(control, FDTrigAction.SecretFound, secretIndex);
    }

    public static List<FDTriggerEntry> GetTriggers(FDControl control, FDTrigAction action, int parameter = -1)
    {
        List<FDTriggerEntry> entries = new();

        foreach (List<FDEntry> entryList in control.Entries.Values)
        {
            foreach (FDEntry entry in entryList)
            {
                if (entry is FDTriggerEntry triggerEntry)
                {
                    int itemIndex = triggerEntry.TrigActionList.FindIndex
                    (
                        i =>
                            i.TrigAction == action && (parameter == -1 || i.Parameter == parameter)
                    );
                    if (itemIndex != -1)
                    {
                        entries.Add(triggerEntry);
                    }
                }
            }
        }

        return entries;
    }

    public static List<FDActionListItem> GetActionListItems(FDControl control, FDTrigAction trigAction, int sectorIndex = -1)
    {
        List<FDActionListItem> items = new();

        List<List<FDEntry>> entrySearch;
        if (sectorIndex == -1)
        {
            entrySearch = control.Entries.Values.ToList();
        }
        else
        {
            entrySearch = new List<List<FDEntry>>
            {
                control.Entries[sectorIndex]
            };
        }

        foreach (List<FDEntry> entryList in entrySearch)
        {
            foreach (FDEntry entry in entryList)
            {
                if (entry is FDTriggerEntry triggerEntry)
                {
                    foreach (FDActionListItem item in triggerEntry.TrigActionList)
                    {
                        if (item.TrigAction == trigAction)
                        {
                            items.Add(item);
                        }
                    }
                }
            }
        }

        return items;
    }

    public static void RemoveEntityTriggers(TR1Level level, int entityIndex, FDControl control)
    {
        foreach (TR1Room room in level.Rooms)
        {
            RemoveEntityTriggers(room.Sectors, entityIndex, control);
        }
    }

    public static void RemoveEntityTriggers(TR2Level level, int entityIndex, FDControl control)
    {
        foreach (TR2Room room in level.Rooms)
        {
            RemoveEntityTriggers(room.SectorList, entityIndex, control);
        }
    }

    public static void RemoveEntityTriggers(TR3Level level, int entityIndex, FDControl control)
    {
        foreach (TR3Room room in level.Rooms)
        {
            RemoveEntityTriggers(room.Sectors, entityIndex, control);
        }
    }

    public static void RemoveEntityTriggers(IEnumerable<TRRoomSector> sectorList, int entityIndex, FDControl control)
    {
        foreach (TRRoomSector sector in sectorList)
        {
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = control.Entries[sector.FDIndex];
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                FDEntry entry = entries[i];
                if (entry is FDTriggerEntry trig)
                {
                    trig.TrigActionList.RemoveAll(a => a.TrigAction == FDTrigAction.Object && a.Parameter == entityIndex);
                    if (trig.TrigActionList.Count == 0)
                    {
                        entries.RemoveAt(i);
                    }
                }
            }

            if (entries.Count == 0)
            {
                // If there isn't anything left, reset the sector to point to the dummy FD
                control.RemoveFloorData(sector);
            }
        }
    }

    public static TRRoomSector GetRoomSector(int x, int z, TRRoomSector[] sectors, TRRoomInfo info, ushort roomDepth)
    {
        int xFloor = (x - info.X) >> TRConsts.WallShift;
        int zFloor = (z - info.Z) >> TRConsts.WallShift;
        return sectors[xFloor * roomDepth + zFloor];
    }

    public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR1Level level, FDControl floorData)
    {
        int xFloor, yFloor;
        TR1Room room = level.Rooms[roomNumber];
        TRRoomSector sector;
        short data;

        do
        {
            // Clip position to edge of tile
            xFloor = (z - room.Info.Z) >> TRConsts.WallShift;
            yFloor = (x - room.Info.X) >> TRConsts.WallShift;

            if (xFloor <= 0)
            {
                xFloor = 0;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (xFloor >= room.NumZSectors - 1)
            {
                xFloor = room.NumZSectors - 1;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (yFloor < 0)
            {
                yFloor = 0;
            }
            else if (yFloor >= room.NumXSectors)
            {
                yFloor = room.NumXSectors - 1;
            }

            sector = room.Sectors[xFloor + yFloor * room.NumZSectors];
            data = GetDoor(sector, floorData);
            if (data != TRConsts.NoRoom && data >= 0 && data < level.Rooms.Count - 1)
            {
                room = level.Rooms[data];
            }
        }
        while (data != TRConsts.NoRoom);

        if (y >= (sector.Floor << 8))
        {
            do
            {
                if (sector.RoomBelow == TRConsts.NoRoom)
                {
                    return sector;
                }

                room = level.Rooms[sector.RoomBelow];
                sector = room.Sectors[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y >= (sector.Floor << 8));
        }
        else if (y < (sector.Ceiling << 8))
        {
            do
            {
                if (sector.RoomAbove == TRConsts.NoRoom)
                {
                    return sector;
                }

                room = level.Rooms[sector.RoomAbove];
                sector = room.Sectors[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y < (sector.RoomAbove << 8));
        }

        return sector;
    }

    public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR2Level level, FDControl floorData)
    {
        int xFloor, yFloor;
        TR2Room room = level.Rooms[roomNumber];
        TRRoomSector sector;
        short data;

        do
        {
            // Clip position to edge of tile
            xFloor = (z - room.Info.Z) >> TRConsts.WallShift;
            yFloor = (x - room.Info.X) >> TRConsts.WallShift;

            if (xFloor <= 0)
            {
                xFloor = 0;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (xFloor >= room.NumZSectors - 1)
            {
                xFloor = room.NumZSectors - 1;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (yFloor < 0)
            {
                yFloor = 0;
            }
            else if (yFloor >= room.NumXSectors)
            {
                yFloor = room.NumXSectors - 1;
            }

            sector = room.SectorList[xFloor + yFloor * room.NumZSectors];
            data = GetDoor(sector, floorData);
            if (data != TRConsts.NoRoom && data >= 0 && data < level.Rooms.Count - 1)
            {
                room = level.Rooms[data];
            }
        }
        while (data != TRConsts.NoRoom);

        if (y >= (sector.Floor << 8))
        {
            do
            {
                if (sector.RoomBelow == TRConsts.NoRoom)
                {
                    return sector;
                }

                room = level.Rooms[sector.RoomBelow];
                sector = room.SectorList[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y >= (sector.Floor << 8));
        }
        else if (y < (sector.Ceiling << 8))
        {
            do
            {
                if (sector.RoomAbove == TRConsts.NoRoom)
                {
                    return sector;
                }

                room = level.Rooms[sector.RoomAbove];
                sector = room.SectorList[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y < (sector.RoomAbove << 8));
        }

        return sector;
    }

    public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR3Level level, FDControl floorData)
    {
        int xFloor, yFloor;
        TR3Room room = level.Rooms[roomNumber];
        TRRoomSector sector;
        short data;

        do
        {
            // Clip position to edge of tile
            xFloor = (z - room.Info.Z) >> TRConsts.WallShift;
            yFloor = (x - room.Info.X) >> TRConsts.WallShift;

            if (xFloor <= 0)
            {
                xFloor = 0;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (xFloor >= room.NumZSectors - 1)
            {
                xFloor = room.NumZSectors - 1;
                if (yFloor < 1)
                {
                    yFloor = 1;
                }
                else if (yFloor > room.NumXSectors - 2)
                {
                    yFloor = room.NumXSectors - 2;
                }
            }
            else if (yFloor < 0)
            {
                yFloor = 0;
            }
            else if (yFloor >= room.NumXSectors)
            {
                yFloor = room.NumXSectors - 1;
            }

            sector = room.Sectors[xFloor + yFloor * room.NumZSectors];
            data = GetDoor(sector, floorData);
            if (data != TRConsts.NoRoom && data >= 0 && data < level.Rooms.Count - 1)
            {
                room = level.Rooms[data];
            }
        }
        while (data != TRConsts.NoRoom);

        if (y >= (sector.Floor << 8))
        {
            do
            {
                if (sector.RoomBelow == TRConsts.NoRoom)
                {
                    return sector;
                }

                int triCheck = CheckFloorTriangle(floorData, sector, x, z);
                if (triCheck == 1)
                {
                    break;
                }
                else if (triCheck == -1 && y < room.Info.YBottom)
                {
                    break;
                }

                room = level.Rooms[sector.RoomBelow];
                sector = room.Sectors[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y >= (sector.Floor << 8));
        }
        else if (y < (sector.Ceiling << 8))
        {
            do
            {
                if (sector.RoomAbove == TRConsts.NoRoom)
                {
                    return sector;
                }

                int triCheck = CheckCeilingTriangle(floorData, sector, x, z);
                if (triCheck == 1)
                {
                    break;
                }
                else if (triCheck == -1 && y >= room.Info.YTop)
                {
                    break;
                }

                room = level.Rooms[sector.RoomAbove];
                sector = room.Sectors[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y < (sector.RoomAbove << 8));
        }

        return sector;
    }

    public static short GetDoor(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex == 0)
        {
            return TRConsts.NoRoom;
        }

        List<FDEntry> entries = floorData.Entries[sector.FDIndex];
        foreach (FDEntry entry in entries)
        {
            if (entry is FDPortalEntry portal)
            {
                return (short)portal.Room;
            }
        }

        return TRConsts.NoRoom;
    }

    private static int CheckFloorTriangle(FDControl floorData, TRRoomSector sector, int x, int z)
    {
        if (sector.FDIndex == 0)
        {
            return 0; 
        }

        if (floorData.Entries[sector.FDIndex].Find(e => e is TR3TriangulationEntry) is TR3TriangulationEntry triangulation)
        {
            FDFunctions func = (FDFunctions)triangulation.Setup.Value;
            int dx = x & (TRConsts.Step4 - 1);
            int dz = z & (TRConsts.Step4 - 1);

            if (func == FDFunctions.FloorTriangulationNWSE_SW && dx <= (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunctions.FloorTriangulationNWSE_NE && dx > (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunctions.FloorTriangulationNESW_SE && dx <= dz)
            {
                return -1;
            }
            else if (func == FDFunctions.FloorTriangulationNESW_NW && dx > dz)
            {
                return -1;
            }

            return 1; // Bad floor data
        }

        return 0;
    }

    private static int CheckCeilingTriangle(FDControl floorData, TRRoomSector sector, int x, int z)
    {
        if (sector.FDIndex == 0)
        {
            return 0;
        }

        if (floorData.Entries[sector.FDIndex].Find(e => e is TR3TriangulationEntry) is TR3TriangulationEntry triangulation)
        {
            FDFunctions func = (FDFunctions)triangulation.Setup.Value;
            int dx = x & (TRConsts.Step4 - 1);
            int dz = z & (TRConsts.Step4 - 1);

            if (func == FDFunctions.CeilingTriangulationNW_SW && dx <= (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunctions.CeilingTriangulationNW_NE && dx > (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunctions.CeilingTriangulationNE_NW && dx <= dz)
            {
                return -1;
            }
            else if (func == FDFunctions.CeilingTriangulationNE_SE && dx > dz)
            {
                return -1;
            }

            return 1; // Bad floor data
        }

        return 0;
    }

    public static int GetHeight(int x, int z, short roomIndex, TR1Level level, FDControl floorData, bool waterOnly)
    {
        TR1Room room = level.Rooms[roomIndex];
        if (waterOnly && !room.ContainsWater)
        {
            return TRConsts.NoHeight;
        }

        TRRoomSector baseSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        TRRoomSector floorSector = baseSector;
        while (floorSector.RoomBelow != TRConsts.NoRoom)
        {
            room = level.Rooms[floorSector.RoomBelow];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            floorSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        }

        TRRoomSector ceilingSector = baseSector;
        while (ceilingSector.RoomAbove != TRConsts.NoRoom)
        {
            room = level.Rooms[ceilingSector.RoomAbove];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            ceilingSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        }

        return GetHeight(x, z, floorSector, ceilingSector, floorData);
    }

    public static int GetHeight(int x, int z, short roomIndex, TR2Level level, FDControl floorData, bool waterOnly)
    {
        TR2Room room = level.Rooms[roomIndex];
        if (waterOnly && !room.ContainsWater)
        {
            return TRConsts.NoHeight;
        }

        TRRoomSector baseSector = GetRoomSector(x, z, room.SectorList, room.Info, room.NumZSectors);
        TRRoomSector floorSector = baseSector;
        while (floorSector.RoomBelow != TRConsts.NoRoom)
        {
            room = level.Rooms[floorSector.RoomBelow];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            floorSector = GetRoomSector(x, z, room.SectorList, room.Info, room.NumZSectors);
        }

        TRRoomSector ceilingSector = baseSector;
        while (ceilingSector.RoomAbove != TRConsts.NoRoom)
        {
            room = level.Rooms[ceilingSector.RoomAbove];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            ceilingSector = GetRoomSector(x, z, room.SectorList, room.Info, room.NumZSectors);
        }

        return GetHeight(x, z, floorSector, ceilingSector, floorData);
    }

    public static int GetHeight(int x, int z, short roomIndex, TR3Level level, FDControl floorData, bool waterOnly)
    {
        TR3Room room = level.Rooms[roomIndex];
        if (waterOnly && !room.ContainsWater)
        {
            return TRConsts.NoHeight;
        }

        TRRoomSector baseSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        TRRoomSector floorSector = baseSector;
        while (floorSector.RoomBelow != TRConsts.NoRoom)
        {
            room = level.Rooms[floorSector.RoomBelow];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            floorSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        }

        TRRoomSector ceilingSector = baseSector;
        while (ceilingSector.RoomAbove != TRConsts.NoRoom)
        {
            room = level.Rooms[ceilingSector.RoomAbove];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            ceilingSector = GetRoomSector(x, z, room.Sectors, room.Info, room.NumZSectors);
        }

        return GetHeight(x, z, floorSector, ceilingSector, floorData);
    }

    public static int GetHeight(int x, int z, TRRoomSector floorSector, TRRoomSector ceilingSector, FDControl floorData)
    {
        int floor = TRConsts.Step1 * floorSector.Floor;
        int ceiling = TRConsts.Step1 * ceilingSector.Ceiling;

        floor += GetHeightAdjustment(x, z, FDSlantEntryType.FloorSlant, floorSector, floorData);
        ceiling += GetHeightAdjustment(x, z, FDSlantEntryType.CeilingSlant, ceilingSector, floorData);

        int height = Math.Abs(floor - ceiling);
        return height + height % 2;
    }

    public static int GetHeightAdjustment(int x, int z, FDSlantEntryType slantType, TRRoomSector sector, FDControl floorData)
    {
        int adjustment = 0;
        if (sector.FDIndex == 0)
        {
            return adjustment;
        }

        FDEntry entry = floorData.Entries[sector.FDIndex].Find(e => e is FDSlantEntry s && s.Type == slantType);
        if (entry is not FDSlantEntry slant)
        {
            return adjustment;
        }

        sbyte xoff = slant.XSlant;
        sbyte zoff = slant.ZSlant;

        if (xoff < 0)
        {
            if (slantType == FDSlantEntryType.FloorSlant)
            {
                adjustment -= (xoff * (x & (TRConsts.Step4 - 1))) >> 2;
            }
            else
            {
                adjustment += (xoff * ((TRConsts.Step4 - 1 - x) & (TRConsts.Step4 - 1))) >> 2;
            }
        }
        else
        {
            if (slantType == FDSlantEntryType.FloorSlant)
            {
                adjustment += (xoff * ((TRConsts.Step4 - 1 - x) & (TRConsts.Step4 - 1))) >> 2;
            }
            else
            {
                adjustment -= (xoff * (x & (TRConsts.Step4 - 1))) >> 2;
            }
        }

        if (zoff < 0)
        {
            if (slantType == FDSlantEntryType.FloorSlant)
            {
                adjustment -= (zoff * (z & (TRConsts.Step4 - 1))) >> 2;
            }
            else
            {
                adjustment += (zoff * (z & (TRConsts.Step4 - 1))) >> 2;
            }
        }
        else
        {
            if (slantType == FDSlantEntryType.FloorSlant)
            {
                adjustment += (zoff * ((TRConsts.Step4 - 1 - z) & (TRConsts.Step4 - 1))) >> 2;
            }
            else
            {
                adjustment -= (zoff * ((TRConsts.Step4 - 1 - z) & (TRConsts.Step4 - 1))) >> 2;
            }
        }

        return adjustment;
    }
}
