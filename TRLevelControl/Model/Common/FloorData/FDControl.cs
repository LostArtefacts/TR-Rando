using System.Collections;
using TRLevelControl.Build;

namespace TRLevelControl.Model;

public class FDControl : IEnumerable<KeyValuePair<int, List<FDEntry>>>
{
    private readonly TRGameVersion _version;
    private readonly ushort _dummyEntry;
    private SortedDictionary<int, List<FDEntry>> _entries;

    public List<FDEntry> this[int index]
    {
        get => _entries[index];
        set => _entries[index] = value;
    }

    public bool ContainsKey(int index)
        => _entries.ContainsKey(index);

    public IEnumerator<KeyValuePair<int, List<FDEntry>>> GetEnumerator()
        => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public IEnumerable<FDEntry> FindAll(Func<FDEntry, bool> predicate)
    {
        return _entries.Values.SelectMany(v => v.Where(predicate));
    }

    public FDControl(TRGameVersion version, ushort dummyData = 0)
    {
        _version = version;
        _dummyEntry = dummyData;
        _entries = new();
    }

    public void CreateFloorData(TRRoomSector sector)
    {
        int index;
        if (_entries.Count == 0)
        {
            index = 1;
        }
        else
        {
            TRFDBuilder builder = new(_version);
            index = _entries.Keys.Last();
            index += builder.Flatten(_entries[index]).Count;
        }

        _entries.Add(index, new List<FDEntry>());
        sector.FDIndex = (ushort)index;
    }

    public List<ushort> Flatten(IEnumerable<TRRoomSector> sectors)
    {
        List<ushort> data = new()
        {
            _dummyEntry
        };

        // Flatten each entry list and map old indices to new.
        TRFDBuilder builder = new(_version);
        Dictionary<int, int> newIndices = new();
        foreach (int currentIndex in _entries.Keys)
        {
            List<ushort> sectorData = builder.Flatten(_entries[currentIndex]);
            if (sectorData.Count > 0)
            {
                newIndices.Add(currentIndex, data.Count);
                data.AddRange(sectorData);
            }
        }

        // Update each TRRoomSector by repointing its FDIndex value to the newly calculated value.
        SortedDictionary<int, List<FDEntry>> _updatedEntries = new();
        foreach (TRRoomSector sector in sectors)
        {
            ushort index = sector.FDIndex;
            if (newIndices.ContainsKey(index))
            {
                sector.FDIndex = (ushort)newIndices[index];

                // Map the list of entries against the new index
                _updatedEntries[sector.FDIndex] = _entries[index];
            }
            else if (_entries.ContainsKey(index))
            {
                // FD has been removed - we only reset it if it was a valid
                // previous entry, because some levels (TRUB for ex) have stale
                // data. This keeps tests happy.
                sector.FDIndex = 0;
            }
        }

        // Update the stored values in case of further changes
        _entries = _updatedEntries;
        return data;
    }

    public List<FDTriggerEntry> GetEntityTriggers(int entityIndex)
    {
        return GetTriggers(FDTrigAction.Object, entityIndex);
    }

    public List<FDTriggerEntry> GetSecretTriggers(int secretIndex)
    {
        return GetTriggers(FDTrigAction.SecretFound, secretIndex);
    }

    public List<FDTriggerEntry> GetTriggers(FDTrigAction action, int parameter = -1)
    {
        return _entries.Values
            .SelectMany(e => e.Where(i => i is FDTriggerEntry))
            .Cast<FDTriggerEntry>()
            .Where(t => t.Actions.Find(a => a.Action == action && (parameter == -1 || a.Parameter == parameter)) != null)
            .ToList();
    }

    public List<FDActionItem> GetActionItems(FDTrigAction action, int sectorIndex = -1)
    {
        List<List<FDEntry>> entrySearch;
        if (sectorIndex == -1)
        {
            entrySearch = _entries.Values.ToList();
        }
        else
        {
            entrySearch = new List<List<FDEntry>>
            {
                _entries[sectorIndex]
            };
        }

        return entrySearch
            .SelectMany(e => e.Where(i => i is FDTriggerEntry))
            .Cast<FDTriggerEntry>()
            .SelectMany(t => t.Actions.FindAll(a => a.Action == action))
            .ToList();
    }

    public void RemoveEntityTriggers(IEnumerable<TRRoomSector> sectorList, int entityIndex)
    {
        foreach (TRRoomSector sector in sectorList)
        {
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = _entries[sector.FDIndex];
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                FDEntry entry = entries[i];
                if (entry is not FDTriggerEntry trig)
                {
                    continue;
                }

                trig.Actions.RemoveAll(a => a.Action == FDTrigAction.Object && a.Parameter == entityIndex);
                if (trig.Actions.Count == 0)
                {
                    entries.RemoveAt(i);
                }
            }
        }
    }

    public TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR1Level level)
        => GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR2Level level)
        => GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR3Level level)
        => GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public TRRoomSector GetRoomSector<R>(int x, int y, int z, short roomNumber, List<R> rooms)
        where R : TRRoom
    {
        int xFloor, yFloor;
        TRRoom room = rooms[roomNumber];
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
            data = GetDoor(sector);
            if (data != TRConsts.NoRoom && data >= 0 && data < rooms.Count - 1)
            {
                room = rooms[data];
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

                int triCheck = CheckFloorTriangle(sector, x, z);
                if (triCheck == 1)
                {
                    break;
                }
                else if (triCheck == -1 && y < room.Info.YBottom)
                {
                    break;
                }

                room = rooms[sector.RoomBelow];
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

                int triCheck = CheckCeilingTriangle(sector, x, z);
                if (triCheck == 1)
                {
                    break;
                }
                else if (triCheck == -1 && y >= room.Info.YTop)
                {
                    break;
                }

                room = rooms[sector.RoomAbove];
                sector = room.Sectors[((z - room.Info.Z) >> TRConsts.WallShift) + ((x - room.Info.X) >> TRConsts.WallShift) * room.NumZSectors];
            }
            while (y < (sector.RoomAbove << 8));
        }

        return sector;
    }

    public short GetDoor(TRRoomSector sector)
    {
        if (sector.FDIndex == 0)
        {
            return TRConsts.NoRoom;
        }

        List<FDEntry> entries = _entries[sector.FDIndex];
        foreach (FDEntry entry in entries)
        {
            if (entry is FDPortalEntry portal)
            {
                return portal.Room;
            }
        }

        return TRConsts.NoRoom;
    }

    private int CheckFloorTriangle(TRRoomSector sector, int x, int z)
    {
        if (sector.FDIndex == 0)
        {
            return 0;
        }

        if (_entries[sector.FDIndex].Find(e => e is FDTriangulationEntry) is FDTriangulationEntry triangulation)
        {
            FDFunction func = triangulation.GetFunction();
            int dx = x & TRConsts.WallMask;
            int dz = z & TRConsts.WallMask;

            if (func == FDFunction.FloorTriangulationNWSE_SW && dx <= (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunction.FloorTriangulationNWSE_NE && dx > (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunction.FloorTriangulationNESW_SE && dx <= dz)
            {
                return -1;
            }
            else if (func == FDFunction.FloorTriangulationNESW_NW && dx > dz)
            {
                return -1;
            }

            return 1; // Bad floor data
        }

        return 0;
    }

    private int CheckCeilingTriangle(TRRoomSector sector, int x, int z)
    {
        if (sector.FDIndex == 0)
        {
            return 0;
        }

        if (_entries[sector.FDIndex].Find(e => e is FDTriangulationEntry) is FDTriangulationEntry triangulation)
        {
            FDFunction func = triangulation.GetFunction();
            int dx = x & TRConsts.WallMask;
            int dz = z & TRConsts.WallMask;

            if (func == FDFunction.CeilingTriangulationNW_SW && dx <= (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunction.CeilingTriangulationNW_NE && dx > (TRConsts.Step4 - dz))
            {
                return -1;
            }
            else if (func == FDFunction.CeilingTriangulationNE_NW && dx <= dz)
            {
                return -1;
            }
            else if (func == FDFunction.CeilingTriangulationNE_SE && dx > dz)
            {
                return -1;
            }

            return 1; // Bad floor data
        }

        return 0;
    }

    public int GetHeight<T>(int x, int z, int roomIndex, IEnumerable<T> allRooms, bool waterOnly)
        where T : TRRoom
    {
        int floor = GetFloorHeight(x, z, roomIndex, allRooms, waterOnly);
        int ceiling = GetCeilingHeight(x, z, roomIndex, allRooms, waterOnly);

        if (floor == TRConsts.NoHeight || ceiling == TRConsts.NoHeight)
        {
            return TRConsts.NoHeight;
        }

        return Math.Abs(floor - ceiling);
    }

    public int GetFloorHeight<T>(int x, int z, int roomIndex, IEnumerable<T> allRooms, bool waterOnly)
        where T : TRRoom
    {
        List<T> rooms = allRooms.ToList();

        TRRoom room = rooms[roomIndex];
        if (waterOnly && !room.ContainsWater)
        {
            return TRConsts.NoHeight;
        }

        TRRoomSector sector = room.GetSector(x, z);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            room = rooms[sector.RoomBelow];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            sector = room.GetSector(x, z);
        }

        int floor = TRConsts.Step1 * sector.Floor;
        floor += GetHeightAdjustment(x, z, FDSlantType.Floor, sector);

        if (_version >= TRGameVersion.TR3)
        {
            floor += GetTriangulationFloor(x, z, sector);
        }

        return floor;
    }

    public int GetCeilingHeight<T>(int x, int z, int roomIndex, IEnumerable<T> allRooms, bool waterOnly)
        where T : TRRoom
    {
        List<T> rooms = allRooms.ToList();

        TRRoom room = rooms[roomIndex];
        if (waterOnly && !room.ContainsWater)
        {
            return TRConsts.NoHeight;
        }

        TRRoomSector sector = room.GetSector(x, z);
        while (sector.RoomAbove != TRConsts.NoRoom)
        {
            room = rooms[sector.RoomAbove];
            if (waterOnly && !room.ContainsWater)
            {
                break;
            }
            sector = room.GetSector(x, z);
        }

        int ceiling = TRConsts.Step1 * sector.Ceiling;
        ceiling += GetHeightAdjustment(x, z, FDSlantType.Ceiling, sector);

        if (_version >= TRGameVersion.TR3)
        {
            ceiling += GetTriangulationCeiling(x, z, sector);
        }

        return ceiling;
    }

    public int GetHeightAdjustment(int x, int z, FDSlantType slantType, TRRoomSector sector)
    {
        int adjustment = 0;
        if (sector.FDIndex == 0)
        {
            return adjustment;
        }

        FDEntry entry = this[sector.FDIndex].Find(e => e is FDSlantEntry s && s.Type == slantType);
        if (entry is not FDSlantEntry slant)
        {
            return adjustment;
        }

        sbyte xoff = slant.XSlant;
        sbyte zoff = slant.ZSlant;

        if (xoff < 0)
        {
            if (slantType == FDSlantType.Floor)
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
            if (slantType == FDSlantType.Floor)
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
            if (slantType == FDSlantType.Floor)
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
            if (slantType == FDSlantType.Floor)
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

    public int GetTriangulationFloor(int x, int z, TRRoomSector sector)
    {
        int adjustment = 0;
        if (sector.FDIndex == 0)
        {
            return adjustment;
        }

        FDEntry entry = this[sector.FDIndex].Find(e => e is FDTriangulationEntry t && t.IsFloorTriangulation);
        if (entry is not FDTriangulationEntry triangulation)
        {
            return adjustment;
        }

        int t0 = triangulation.C10;
        int t1 = triangulation.C00;
        int t2 = triangulation.C01;
        int t3 = triangulation.C11;
        int dx = x & (TRConsts.Step4 - 1);
        int dz = z & (TRConsts.Step4 - 1);
        int xoff = 0;
        int zoff = 0;

        if (triangulation.Type != FDTriangulationType.FloorNWSE_Solid
            && triangulation.Type != FDTriangulationType.FloorNWSE_SW
            && triangulation.Type != FDTriangulationType.FloorNWSE_NE)
        {
            if (dx <= dz)
            {
                int hadj = triangulation.H2;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t2 - t1;
                zoff = t3 - t2;
            }
            else
            {
                int hadj = triangulation.H1;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t3 - t0;
                zoff = t0 - t1;
            }
        }
        else
        {
            if (dx <= (TRConsts.Step4 - dz))
            {
                int hadj = triangulation.H2;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t2 - t1;
                zoff = t0 - t1;
            }
            else
            {
                int hadj = triangulation.H1;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t3 - t0;
                zoff = t3 - t2;
            }
        }

        if (xoff < 0)
            adjustment -= xoff * (z & (TRConsts.Step4 - 1)) >> 2;
        else
            adjustment += xoff * ((-1 - (ushort)z) & (TRConsts.Step4 - 1)) >> 2;

        if (zoff < 0)
            adjustment -= zoff * (x & (TRConsts.Step4 - 1)) >> 2;
        else
            adjustment += zoff * ((-1 - (ushort)x) & (TRConsts.Step4 - 1)) >> 2;

        return adjustment;
    }

    public int GetTriangulationCeiling(int x, int z, TRRoomSector sector)
    {
        int adjustment = 0;
        if (sector.FDIndex == 0)
        {
            return adjustment;
        }

        FDEntry entry = this[sector.FDIndex].Find(e => e is FDTriangulationEntry t && t.IsCeilingTriangulation);
        if (entry is not FDTriangulationEntry triangulation)
        {
            return adjustment;
        }

        const int wallMask = TRConsts.Step4 - 1;
        int t0 = -triangulation.C10;
        int t1 = -triangulation.C00;
        int t2 = -triangulation.C01;
        int t3 = -triangulation.C11;
        int dx = x & wallMask;
        int dz = z & wallMask;
        int xoff = 0;
        int zoff = 0;

        if (triangulation.Type == FDTriangulationType.CeilingNWSE_Solid
            || triangulation.Type == FDTriangulationType.CeilingNWSE_SW
            || triangulation.Type == FDTriangulationType.CeilingNWSE_NE)
        {
            if (dx <= TRConsts.Step4 - dz)
            {
                int hadj = triangulation.H2;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t2 - t1;
                zoff = t3 - t2;
            }
            else
            {
                int hadj = triangulation.H1;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t3 - t0;
                zoff = t0 - t1;
            }
        }
        else
        {
            if (dx <= dz)
            {
                int hadj = triangulation.H2;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t2 - t1;
                zoff = t0 - t1;
            }
            else
            {
                int hadj = triangulation.H1;
                if ((hadj & 0x10) != 0)
                {
                    hadj |= 0xFFF0;
                }

                adjustment += hadj << 8;
                xoff = t3 - t0;
                zoff = t3 - t2;
            }
        }

        if (xoff < 0)
            adjustment += ((z & wallMask) * xoff) >> 2;
        else
            adjustment -= ((-1 - (z & wallMask)) * xoff) >> 2;

        if (zoff < 0)
            adjustment += ((-1 - (x & wallMask)) * zoff) >> 2;
        else
            adjustment -= ((x & wallMask) * zoff) >> 2;

        return adjustment;
    }
}
