using System.Numerics;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public abstract class AbstractLocationGenerator<T, L>
    where L : TRLevelBase
    where T : Enum
{
    protected static readonly int _standardHeight = TRConsts.Step3;
    protected static readonly int _crawlspaceHeight = TRConsts.Step2;
    protected static readonly int _slantLimit = 3;
    protected static readonly int _fenceMinLimit = TRConsts.Step1;
    protected static readonly int _fenceMaxLimit = TRConsts.Step4 - 24;

    protected FDControl _floorData;
    protected ISet<TRRoomSector> _excludedSectors, _antiExcludedSectors; // Anti-excluded => not necessarily included

    public List<Location> Generate(L level, List<Location> exclusions, bool keyItemMode = false)
    {
        _floorData = level.FloorData;

        // Manual exclusions or known rooms such as swamps we wish to eliminate first
        DetermineBaseExcludedSectors(level, exclusions, keyItemMode);

        // Check all remaining sectors and build a list of valid locations
        return GetValidLocations(level);
    }

    protected void DetermineBaseExcludedSectors(L level, List<Location> exclusions, bool keyItemMode)
    {
        _excludedSectors = new HashSet<TRRoomSector>();
        _antiExcludedSectors = new HashSet<TRRoomSector>();

        // Anti-exclusions are used where we have invalidated an entire room, but we
        // wish to retain a handful of specific locations. These sectors are still
        // subject to static mesh checks and the standard checks in GetValidLocations.
        List<Location> antiExclusions = exclusions.FindAll(l => !l.Validated);
        foreach (Location location in antiExclusions)
        {
            _antiExcludedSectors.Add(GetSector(location, level));
        }

        // Check all other room or individual location exclusions. If TargetType is set,
        // it applies only to key item mode.
        List<Location> mainExclusions = exclusions.FindAll(
            l => l.Validated && (l.TargetType == -1 || keyItemMode));
        foreach (Location location in mainExclusions)
        {
            if (location.InvalidatesRoom)
            {
                ExcludeRoom(level, location.Room);
            }
            else
            {
                _excludedSectors.Add(GetSector(location, level));
            }
        }

        List<T> collidableMeshes = GetStaticMeshes(level)
            .Where(kvp => !IsPermittableMesh(kvp.Value))
            .Select(kvp => kvp.Key)
            .ToList();

        for (short r = 0; r < GetRoomCount(level); r++)
        {
            // Exclude flipped rooms
            short flipRoom = GetFlipMapRoom(level, r);
            if (flipRoom != -1)
            {
                ExcludeRoom(level, flipRoom);
                ExcludeFlipSectors(level, r, flipRoom);
            }

            // Exclude the room if it has no collisional portals
            if (!RoomHasCollisionalPortal(level, r))
            {
                ExcludeRoom(level, r);
                continue;
            }

            // Allow subclasses to determine specifics e.g. swamps
            if (!IsRoomValid(level, r))
            {
                ExcludeRoom(level, r);
                continue;
            }

            // If there are any collidable static meshes in this room, exclude the sectors they're on
            Dictionary<T, List<Location>> meshLocations = GetRoomStaticMeshLocations(level, r);
            foreach (var (type, locations) in meshLocations)
            {
                if (collidableMeshes.Contains(type))
                {
                    foreach (Location location in locations)
                    {
                        _excludedSectors.Add(GetSector(location, level));
                    }
                }
            }
        }
    }

    private static bool IsPermittableMesh(TRStaticMesh mesh)
    {
        // TR1/2 use a hitbox flag
        if (mesh.NonCollidable)
        {
            return true;
        }

        // TR3+ uses a null hitbox
        int width = mesh.CollisionBox.MaxX - mesh.CollisionBox.MinX;
        int depth = mesh.CollisionBox.MaxZ - mesh.CollisionBox.MinZ;
        if (width == 0 && depth == 0)
        {
            return true;
        }

        // Allow fences/barriers
        if ((width <= _fenceMinLimit && depth >= _fenceMaxLimit)
            || (width >= _fenceMaxLimit && depth <= _fenceMinLimit))
        {
            return true;
        }

        return false;
    }

    private bool RoomHasCollisionalPortal(L level, short room)
    {
        foreach (TRRoomSector sector in GetRoomSectors(level, room))
        {
            // Vertical?
            if (sector.RoomAbove != TRConsts.NoRoom || sector.RoomBelow != TRConsts.NoRoom)
            {
                return true;
            }

            // Horizontal?
            if (sector.FDIndex != 0
                && _floorData[sector.FDIndex].Any(e => e is FDPortalEntry))
            {
                return true;
            }
        }

        short flipRoom = GetFlipMapRoom(level, room);
        if (flipRoom != -1)
        {
            return RoomHasCollisionalPortal(level, flipRoom);
        }

        return false;
    }

    protected void ExcludeRoom(L level, int room)
    {
        // Exclude every sector in this room, unless its location has been
        // marked as an anti-exclusion.
        foreach (TRRoomSector sector in GetRoomSectors(level, room))
        {
            if (!_antiExcludedSectors.Contains(sector))
            {
                _excludedSectors.Add(sector);
            }
        }
    }

    protected void ExcludeFlipSectors(L level, short room, short flipRoom)
    {
        // Exclude sectors in unflipped rooms where the floor isn't
        // identical in the flipped state.
        List<TRRoomSector> roomSectors = GetRoomSectors(level, room);
        List<TRRoomSector> flipSectors = GetRoomSectors(level, flipRoom);

        for (int sectorIndex = 0; sectorIndex < roomSectors.Count; sectorIndex++)
        {
            TRRoomSector roomSector = roomSectors[sectorIndex];
            Location roomLoc = CreateLocation(level, room, sectorIndex, roomSector);
            if (roomLoc == null)
                continue;

            TRRoomSector flipSector = GetSector(roomLoc.X, roomLoc.Z, flipRoom, level);
            int flipIndex = flipSectors.IndexOf(flipSector);

            Location flipLoc = CreateLocation(level, flipRoom, flipIndex, flipSector);
            if (flipLoc == null || roomLoc.ToVector() != flipLoc.ToVector() || flipSector.IsWall)
            {
                _excludedSectors.Add(roomSector);
            }
        }
    }

    private List<Location> GetValidLocations(L level)
    {
        List<Location> locations = new();
        for (short r = 0; r < GetRoomCount(level); r++)
        {
            List<TRRoomSector> sectors = GetRoomSectors(level, r);
            for (int sectorIndex = 0; sectorIndex < sectors.Count; sectorIndex++)
            {
                TRRoomSector sector = sectors[sectorIndex];
                // Basic exclusion checks
                if (IsSectorInvalid(sector))
                {
                    continue;
                }

                Location location = CreateLocation(level, r, sectorIndex, sector);
                if (location != null)
                {
                    locations.Add(location);
                }
            }
        }

        return locations;
    }

    private Location CreateLocation(L level, short roomIndex, int sectorIndex, TRRoomSector sector)
    {
        // Default position adjustments for a flat sector
        int dx = TRConsts.Step2;
        int dz = TRConsts.Step2;
        int dy = 0;
        short angle = -1;

        bool isTrianglePortal = false;

        // Check the floor data, if there is any
        if (sector.FDIndex != 0)
        {
            List<FDEntry> entries = _floorData[sector.FDIndex];
            bool invalidFloorData = false;
            for (int i = 0; i < entries.Count; i++)
            {
                FDEntry entry = entries[i];
                if (entry is FDTriggerEntry trigger)
                {
                    // Basic trigger checks - e.g. end level, underwater current
                    if (IsTriggerInvalid(level, trigger))
                    {
                        invalidFloorData = true;
                        break;
                    }
                }
                else if (entry is FDPortalEntry)
                {
                    invalidFloorData = true;
                    break;
                }
                else if (entry is FDKillLaraEntry)
                {
                    // For obvious reasons
                    invalidFloorData = true;
                    break;
                }
                else if (entry is FDSlantEntry slant && slant.Type == FDSlantType.Floor)
                {
                    // NB It's only ever FDSlantEntry or TR3TriangulationEntry (or neither) for TR3-5                                
                    Vector4? bestMidpoint = GetBestSlantMidpoint(slant);
                    if (bestMidpoint.HasValue)
                    {
                        dx = (int)bestMidpoint.Value.X;
                        dy = (int)bestMidpoint.Value.Y;
                        dz = (int)bestMidpoint.Value.Z;
                        angle = (short)bestMidpoint.Value.W;
                    }
                    else
                    {
                        // Too much of a slope
                        invalidFloorData = true;
                        break;
                    }
                }
                else if (entry is FDTriangulationEntry triangulation && triangulation.IsFloorTriangulation)
                {
                    Vector4? bestMidpoint = GetBestTriangleMidpoint(sector, triangulation, sectorIndex, GetRoomDepth(level, roomIndex), GetRoomYTop(level, roomIndex));
                    if (bestMidpoint.HasValue)
                    {
                        dx = (int)(bestMidpoint.Value.X * TRConsts.Step1);
                        dy = (int)(bestMidpoint.Value.Y * TRConsts.Step1);
                        dz = (int)(bestMidpoint.Value.Z * TRConsts.Step1);
                        angle = (short)bestMidpoint.Value.W;

                        isTrianglePortal = triangulation.IsFloorPortal;
                    }
                    else
                    {
                        // Either both triangles were too sloped, or one was too sloped and the other was
                        // a collisional portal.
                        invalidFloorData = true;
                        break;
                    }
                }
                else if (entry is FDMinecartEntry minecartL && minecartL.Type == FDMinecartType.Left && i < entries.Count - 1 
                    && entries[i + 1] is FDMinecartEntry minecartR && minecartR.Type == FDMinecartType.Right)
                {
                    // Minecart stops here, so block this tile.
                    invalidFloorData = true;
                }
            }

            // Bail out if we don't like the look of the FD here
            if (invalidFloorData)
            {
                return null;
            }
        }

        // Final check for vertical portals - if a triangulation entry was detected and one
        // of the triangles is a portal, we can allow it but if there are no triangles, this
        // sector is in mid-air.
        if (sector.RoomBelow != TRConsts.NoRoom && !isTrianglePortal)
        {
            return null;
        }

        // Make the location and adjust the positioning on this tile
        Location location = CreateLocation(roomIndex, sector, sectorIndex, GetRoomDepth(level, roomIndex), GetRoomPosition(level, roomIndex));
        location.X += dx;
        location.Y += dy;
        location.Z += dz;
        location.Angle = angle;

        if (!WadingAllowed)
        {
            int waterHeight = GetHeight(level, location, true);
            if (waterHeight != TRConsts.NoHeight && waterHeight < _standardHeight)
            {
                return null;
            }
        }

        // Check the absolute height from floor to ceiling to ensure Lara can be here.
        // Test around the mid-point for cases of steep ceiling slopes.
        Location testLocation = new()
        {
            Y = location.Y,
            Room = location.Room
        };
        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                testLocation.X = location.X + x * TRConsts.Step1 / 2;
                testLocation.Z = location.Z + z * TRConsts.Step1 / 2;
                int height = GetHeight(level, testLocation, false);
                if (height < (CrawlspacesAllowed ? _crawlspaceHeight : _standardHeight))
                {
                    return null;
                }
            }
        }

        return location;
    }

    private bool IsSectorInvalid(TRRoomSector sector)
    {
        // Inside a wall, on a "normal" slope, or too near the ceiling
        return _excludedSectors.Contains(sector)
            || sector.Floor == TRConsts.WallClicks
            || sector.Ceiling == TRConsts.WallClicks
            || sector.IsSlipperySlope;
    }

    private bool IsTriggerInvalid(L level, FDTriggerEntry trigger)
    {
        // Any trigger types where we don't want items placed
        return trigger.Actions.Any(a =>
            a.Action == FDTrigAction.UnderwaterCurrent
            || a.Action == FDTrigAction.EndLevel
        ) || !TriggerSupportsItems(level, trigger);
    }

    protected virtual bool TriggerSupportsItems(L level, FDTriggerEntry trigger)
    {
        return true;
    }

    public Vector4? GetBestSlantMidpoint(FDSlantEntry slant)
    {
        if (Math.Abs(slant.XSlant) > 2 || Math.Abs(slant.ZSlant) > 2)
        {
            // This is a slippery slope
            return null;
        }

        List<sbyte> corners = new() { 0, 0, 0, 0 };
        if (slant.XSlant > 0)
        {
            corners[0] += slant.XSlant;
            corners[1] += slant.XSlant;
        }
        else if (slant.XSlant < 0)
        {
            corners[2] -= slant.XSlant;
            corners[3] -= slant.XSlant;
        }

        if (slant.ZSlant > 0)
        {
            corners[0] += slant.ZSlant;
            corners[2] += slant.ZSlant;
        }
        else if (slant.ZSlant < 0)
        {
            corners[1] -= slant.ZSlant;
            corners[3] -= slant.ZSlant;
        }

        int uniqueHeights = corners.ToHashSet().Count;
        int dy = corners.Max() - corners.Min();
        int angle = -1;

        if (uniqueHeights == 2)
        {
            // This is a regular slope with 2 sets of equal corner heights.
            // Can Lara stand here?
            if (dy > _slantLimit)
            {
                return null;
            }

            if (corners[0] == corners[1])
            {
                angle = corners[0] < corners[2] ? 16384 : -16384;
            }
            else
            {
                angle = corners[2] < corners[3] ? 0 : -32768;
            }
        }
        else
        {
            // Slant limit goes up by one - any more and it's a slippery slope so
            // will already have been excluded.
            if (dy > _slantLimit + 1)
            {
                return null;
            }
        }

        dy = (dy * TRConsts.Step1) / 2; // Half-way down the slope
        return new Vector4(TRConsts.Step2, dy, TRConsts.Step2, angle);
    }

    // Change to GetHeight
    // Returned vector contains x, y, z and angle adjustments for midpoint
    private static Vector4? GetBestTriangleMidpoint(TRRoomSector sector, FDTriangulationEntry triangulation, int sectorIndex, int roomDepth, int roomYTop)
    {
        int t0 = triangulation.C10;
        int t1 = triangulation.C00;
        int t2 = triangulation.C01;
        int t3 = triangulation.C11;

        List<byte> triangleCorners = new()
        {
            triangulation.C00,
            triangulation.C01,
            triangulation.C10,
            triangulation.C11
        };

        int max = triangleCorners.Max();
        List<int> corners = new()
        {
            max - triangleCorners[0],
            max - triangleCorners[1],
            max - triangleCorners[2],
            max - triangleCorners[3]
        };

        List<Vector3> triangle1, triangle2;

        int x1, x2, dx1, dx2;
        int z1, z2, dz1, dz2;
        int xoff1, zoff1, xoff2, zoff2;
        int xOffset = int.MaxValue, zOffset = int.MaxValue, angle = -1;
        Vector3 triSum1, triSum2;
        Vector4? bestMatch = null;

        int sectorXPos = sectorIndex / roomDepth * TRConsts.Step4;
        int sectorZPos = sectorIndex % roomDepth * TRConsts.Step4;

        FDFunction func = triangulation.GetFunction();
        switch (func)
        {
            case FDFunction.FloorTriangulationNWSE_Solid:
            case FDFunction.FloorTriangulationNWSE_SW:
            case FDFunction.FloorTriangulationNWSE_NE:
                triangle1 = new List<Vector3>
                {
                    new(0, corners[0], 0),
                    new(0, corners[1], 4),
                    new(4, corners[2], 0)
                };
                triangle2 = new List<Vector3>
                {
                    new(0, corners[1], 4),
                    new(4, corners[2], 0),
                    new(4, corners[3], 4)
                };

                triSum1 = (triangle1[0] + triangle1[1] + triangle1[2]) / 3;
                triSum2 = (triangle2[0] + triangle2[1] + triangle2[2]) / 3;

                x1 = (int)(sectorXPos + triSum1.X * TRConsts.Step1);
                z1 = (int)(sectorZPos + triSum1.Z * TRConsts.Step1);

                // Which quarter of the tile are we in?
                dx1 = x1 & (TRConsts.Step4 - 1);
                dz1 = z1 & (TRConsts.Step4 - 1);

                // Is this the top triangle?
                if (dx1 <= (TRConsts.Step4 - dz1))
                {
                    xoff1 = t2 - t1;
                    zoff1 = t0 - t1;
                }
                else
                {
                    xoff1 = t3 - t0;
                    zoff1 = t3 - t2;
                }

                x2 = (int)(sectorXPos + triSum2.X * TRConsts.Step1);
                z2 = (int)(sectorZPos + triSum2.Z * TRConsts.Step1);
                dx2 = x2 & (TRConsts.Step4 - 1);
                dz2 = z2 & (TRConsts.Step4 - 1);

                if (dx2 <= (TRConsts.Step4 - dz2))
                {
                    xoff2 = t2 - t1;
                    zoff2 = t0 - t1;
                }
                else
                {
                    xoff2 = t3 - t0;
                    zoff2 = t3 - t2;
                }

                // Eliminate hidden flat triangles on shore lines, otherwise the location will be OOB.
                // See geometry in room 44 in Coastal Village for example.
                if (sector.Floor * TRConsts.Step1 == roomYTop)
                {
                    if (xoff1 == 0 && zoff1 == 0)
                    {
                        xoff1 = zoff1 = int.MaxValue;
                    }
                    if (xoff2 == 0 && zoff2 == 0)
                    {
                        xoff2 = zoff2 = int.MaxValue;
                    }
                }

                // Pick a suitable angle for the incline
                if (Math.Abs(zoff1) < Math.Abs(zoff2))
                {
                    angle = -24576;
                }
                else if (Math.Abs(zoff1) > Math.Abs(zoff2))
                {
                    angle = 8192;
                }

                // Work out which triangle has the smallest gradient. We can only include it if
                // the triangle is not a collisional portal.
                if (Math.Abs(xoff1) < Math.Abs(xoff2) && Math.Abs(zoff1) < Math.Abs(zoff2) && func != FDFunction.FloorTriangulationNWSE_SW)
                {
                    xOffset = xoff1;
                    zOffset = zoff1;
                    bestMatch = new Vector4(triSum1.X, triSum1.Y, triSum1.Z, angle);
                }
                else if (func != FDFunction.FloorTriangulationNWSE_NE)
                {
                    xOffset = xoff2;
                    zOffset = zoff2;
                    bestMatch = new Vector4(triSum2.X, triSum2.Y, triSum2.Z, angle);
                }

                break;

            case FDFunction.FloorTriangulationNESW_Solid:
            case FDFunction.FloorTriangulationNESW_SE:
            case FDFunction.FloorTriangulationNESW_NW:
                triangle1 = new List<Vector3>
                {
                    new(0, corners[0], 0),
                    new(4, corners[2], 0),
                    new(4, corners[3], 4)
                };
                triangle2 = new List<Vector3>
                {
                    new(0, corners[0], 0),
                    new(0, corners[1], 4),
                    new(4, corners[3], 4)
                };

                triSum1 = (triangle1[0] + triangle1[1] + triangle1[2]) / 3;
                triSum2 = (triangle2[0] + triangle2[1] + triangle2[2]) / 3;

                x1 = (int)(sectorXPos + triSum1.X * TRConsts.Step1);
                z1 = (int)(sectorZPos + triSum1.Z * TRConsts.Step1);
                dx1 = x1 & (TRConsts.Step4 - 1);
                dz1 = z1 & (TRConsts.Step4 - 1);

                if (dx1 <= dz1)
                {
                    xoff1 = t2 - t1;
                    zoff1 = t3 - t2;
                }
                else
                {
                    xoff1 = t3 - t0;
                    zoff1 = t0 - t1;
                }

                x2 = (int)(sectorXPos + triSum2.X * TRConsts.Step1);
                z2 = (int)(sectorZPos + triSum2.Z * TRConsts.Step1);
                dx2 = x2 & (TRConsts.Step4 - 1);
                dz2 = z2 & (TRConsts.Step4 - 1);

                if (dx2 <= dz2)
                {
                    xoff2 = t2 - t1;
                    zoff2 = t3 - t2;
                }
                else
                {
                    xoff2 = t3 - t0;
                    zoff2 = t0 - t1;
                }

                if (sector.Floor * TRConsts.Step1 == roomYTop)
                {
                    if (xoff1 == 0 && zoff1 == 0)
                    {
                        xoff1 = zoff1 = int.MaxValue;
                    }
                    if (xoff2 == 0 && zoff2 == 0)
                    {
                        xoff2 = zoff2 = int.MaxValue;
                    }
                }

                if (Math.Abs(xoff1) < Math.Abs(xoff2))
                {
                    angle = 24576;
                }
                else if (Math.Abs(xoff1) > Math.Abs(xoff2))
                {
                    angle = -8192;
                }

                if (Math.Abs(xoff1) < Math.Abs(xoff2) && Math.Abs(zoff1) < Math.Abs(zoff2) && func != FDFunction.FloorTriangulationNESW_NW)
                {
                    xOffset = xoff1;
                    zOffset = zoff1;
                    bestMatch = new Vector4(triSum1.X, triSum1.Y, triSum1.Z, angle);
                }
                else if (func != FDFunction.FloorTriangulationNESW_SE)
                {
                    xOffset = xoff2;
                    zOffset = zoff2;
                    bestMatch = new Vector4(triSum2.X, triSum2.Y, triSum2.Z, angle);
                }

                break;
        }

        // Is the slope suitable?
        if (Math.Abs(xOffset) < _slantLimit && Math.Abs(zOffset) < _slantLimit)
        {
            return bestMatch;
        }

        return null;
    }

    private static Location CreateLocation(short roomIndex, TRRoomSector sector, int sectorIndex, ushort roomDepth, Vector2 roomPosition)
    {
        // Get the sector's position in its room
        int x = sectorIndex / roomDepth * TRConsts.Step4;
        int z = sectorIndex % roomDepth * TRConsts.Step4;
        int y = sector.Floor * TRConsts.Step1;

        // Move into world space
        x += (int)roomPosition.X;
        z += (int)roomPosition.Y;

        return new Location
        {
            X = x,
            Y = y,
            Z = z,
            Room = roomIndex
        };
    }

    protected abstract TRRoomSector GetSector(Location location, L level);
    protected abstract TRRoomSector GetSector(int x, int z, int roomIndex, L level);
    protected abstract List<TRRoomSector> GetRoomSectors(L level, int room);
    protected abstract TRDictionary<T, TRStaticMesh> GetStaticMeshes(L level);
    protected abstract int GetRoomCount(L level);
    protected abstract short GetFlipMapRoom(L level, short room);
    protected abstract bool IsRoomValid(L level, short room);
    protected abstract Dictionary<T, List<Location>> GetRoomStaticMeshLocations(L level, short room);
    protected abstract ushort GetRoomDepth(L level, short room);
    protected abstract int GetRoomYTop(L level, short room);
    protected abstract Vector2 GetRoomPosition(L level, short room);
    protected abstract int GetHeight(L level, Location location, bool waterOnly);
    public abstract bool CrawlspacesAllowed { get; }
    public abstract bool WadingAllowed { get; }
}
