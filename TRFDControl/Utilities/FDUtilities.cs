using System.Collections.Generic;
using System.Linq;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Model;

namespace TRFDControl.Utilities
{
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
            List<FDTriggerEntry> entries = new List<FDTriggerEntry>();

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
            List<FDActionListItem> items = new List<FDActionListItem>();

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

        public static readonly short NO_ROOM = 0xff;
        public static readonly short WALL_SHIFT = 10;

        // See Control.c line 516
        public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR2Level level, FDControl floorData)
        {
            int xFloor, yFloor;
            TR2Room room = level.Rooms[roomNumber];
            TRRoomSector sector;
            short data;

            do
            {
                // Clip position to edge of room (that way doorways are detected)
                xFloor = (z - room.Info.Z) >> WALL_SHIFT;
                yFloor = (x - room.Info.X) >> WALL_SHIFT;

                // Ensure we don't test the corner of a room's floor data, as this can cause problems when 4 rooms join at a point
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

                // If doorway, go through and retest, else move onto next stage
                sector = room.SectorList[xFloor + yFloor * room.NumZSectors];
                data = GetDoor(sector, floorData);
                if (data != NO_ROOM && data >= 0 && data < level.Rooms.Length - 1)
                {
                    room = level.Rooms[data];
                }
            }
            while (data != NO_ROOM);

            // If below floor and pit, go down
            if (y >= (sector.Floor << 8))
            {
                do
                {
                    if (sector.RoomBelow == NO_ROOM)
                    {
                        return sector;
                    }

                    room = level.Rooms[sector.RoomBelow];
                    sector = room.SectorList[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y >= (sector.Floor << 8));
            }
            else if (y < (sector.Ceiling << 8))
            {
                // If above ceiling and skylight, go up
                do
                {
                    if (sector.RoomAbove == NO_ROOM)
                    {
                        return sector;
                    }

                    room = level.Rooms[sector.RoomAbove];
                    sector = room.SectorList[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y < (sector.RoomAbove << 8));
            }

            return sector;
        }

        // See Control.c line 766
        public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR3Level level, FDControl floorData)
        {
            int xFloor, yFloor;
            TR3Room room = level.Rooms[roomNumber];
            TRRoomSector sector;
            short data;

            do
            {
                // Clip position to edge of room (that way doorways are detected)
                xFloor = (z - room.Info.Z) >> WALL_SHIFT;
                yFloor = (x - room.Info.X) >> WALL_SHIFT;

                // Ensure we don't test the corner of a room's floor data, as this can cause problems when 4 rooms join at a point
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

                // If doorway, go through and retest, else move onto next stage
                sector = room.Sectors[xFloor + yFloor * room.NumZSectors];
                data = GetDoor(sector, floorData);
                if (data != NO_ROOM && data >= 0 && data < level.Rooms.Length - 1)
                {
                    room = level.Rooms[data];
                }
            }
            while (data != NO_ROOM);

            // If below floor and pit, go down
            if (y >= (sector.Floor << 8))
            {
                do
                {
                    if (sector.RoomBelow == NO_ROOM)
                    {
                        return sector;
                    }

                    int triCheck = CheckNoColFloorTriangle(floorData, sector, x, z);
                    if (triCheck == 1) // If on collision side of a split quad then stop.
                    {
                        break;
                    }
                    else if (triCheck == -1 && y < room.Info.YBottom)
                    {
                        break;
                    }

                    room = level.Rooms[sector.RoomBelow];
                    sector = room.Sectors[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y >= (sector.Floor << 8));
            }
            else if (y < (sector.Ceiling << 8))
            {
                // If above ceiling and skylight, go up
                do
                {
                    if (sector.RoomAbove == NO_ROOM)
                    {
                        return sector;
                    }

                    int triCheck = CheckNoColCeilingTriangle(floorData, sector, x, z);
                    if (triCheck == 1) // If on collision side of a split quad then stop.
                    {
                        break;
                    }
                    else if (triCheck == -1 && y >= room.Info.YTop)
                    {
                        break;
                    }

                    room = level.Rooms[sector.RoomAbove];
                    sector = room.Sectors[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y < (sector.RoomAbove << 8));
            }

            return sector;
        }

        // See Control.c line 1344
        public static short GetDoor(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex == 0)
            {
                return NO_ROOM;
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            foreach (FDEntry entry in entries)
            {
                if (entry is FDPortalEntry portal)
                {
                    return (short)portal.Room;
                }
            }

            return NO_ROOM;
        }

        private static int CheckNoColFloorTriangle(FDControl floorData, TRRoomSector sector, int x, int z)
        {
            if (sector.FDIndex == 0)
            {
                return 0; 
            }

            /*
             * -------
             * |    /|
             * | T / |     	Split type 1
             * |  /  |
             * | / B |
             * |/    |
             * -------
             * NOCOLF1T == FloorTriangulationNWSE_SW
             * NOCOLF1B == FloorTriangulationNWSE_NE
             * NOCOLF2T == FloorTriangulationNESW_SW
             * NOCOLF2B == FloorTriangulationNESW_NW
             */
            if (floorData.Entries[sector.FDIndex].Find(e => e is TR3TriangulationEntry) is TR3TriangulationEntry triangulation)
            {
                FDFunctions func = (FDFunctions)triangulation.Setup.Value;
                int dx = x & 1023;
                int dz = z & 1023;

                if (func == FDFunctions.FloorTriangulationNWSE_SW && dx <= (1024 - dz))
                {
                    return -1;
                }
                else if (func == FDFunctions.FloorTriangulationNWSE_NE && dx > (1024 - dz))
                {
                    return -1;
                }
                else if (func == FDFunctions.FloorTriangulationNESW_SW && dx <= dz)
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

        private static int CheckNoColCeilingTriangle(FDControl floorData, TRRoomSector sector, int x, int z)
        {
            if (sector.FDIndex == 0)
            {
                return 0;
            }

            /*
             * -------
             * |    /|
             * | T / |     	Split type 3
             * |  /  |
             * | / B |
             * |/    |
             * -------
             * NOCOLC1T == CeilingTriangulationNW_SW
             * NOCOLC1B == CeilingTriangulationNW_NE
             * NOCOLC2T == CeilingTriangulationNE_NW
             * NOCOLC2B == CeilingTriangulationNE_SE
             */
            if (floorData.Entries[sector.FDIndex].Find(e => e is TR3TriangulationEntry) is TR3TriangulationEntry triangulation)
            {
                FDFunctions func = (FDFunctions)triangulation.Setup.Value;
                int dx = x & 1023;
                int dz = z & 1023;

                if (func == FDFunctions.CeilingTriangulationNW_SW && dx <= (1024 - dz))
                {
                    return -1;
                }
                else if (func == FDFunctions.CeilingTriangulationNW_NE && dx > (1024 - dz))
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
    }
}