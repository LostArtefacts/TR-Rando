using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
{
    public abstract class AbstractLocationGenerator<L> where L : class
    {
        protected static readonly byte _noRoom = 255;
        protected static readonly int _fullSectorSize = 1024;
        protected static readonly int _halfSectorSize = 512;
        protected static readonly int _qrtSectorSize = 256;
        protected static readonly int _slantLimit = 3;
        protected static readonly int _fenceMinLimit = 256;
        protected static readonly int _fenceMaxLimit = 1000;

        protected FDControl _floorData;
        protected ISet<TRRoomSector> _excludedSectors, _antiExcludedSectors; // Anti-excluded => not necessarily included

        public List<Location> Generate(L level, List<Location> exclusions)
        {
            _floorData = new FDControl();
            ReadFloorData(level);

            // Manual exclusions or known rooms such as swamps we wish to eliminate first
            DetermineBaseExcludedSectors(level, exclusions);

            // Check all remaining sectors and build a list of valid locations
            return GetValidLocations(level);
        }

        protected void DetermineBaseExcludedSectors(L level, List<Location> exclusions)
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

            // Check all other room or individual location exclusions
            List<Location> mainExclusions = exclusions.FindAll(l => l.Validated);
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

            List<TRStaticMesh> collidableMeshes = GetCollidableStaticMeshes(level);

            for (short r = 0; r < GetRoomCount(level); r++)
            {
                // Exclude flipped rooms
                short flipRoom = GetFlipMapRoom(level, r);
                if (flipRoom != -1)
                {
                    ExcludeRoom(level, flipRoom);
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
                Dictionary<ushort, List<Location>> meshLocations = GetRoomStaticMeshLocations(level, r);
                foreach (ushort meshID in meshLocations.Keys)
                {
                    if (collidableMeshes.Find(m => m.ID == meshID) != null)
                    {
                        foreach (Location location in meshLocations[meshID])
                        {
                            _excludedSectors.Add(GetSector(location, level));
                        }
                    }
                }
            }
        }

        protected List<TRStaticMesh> GetCollidableStaticMeshes(L level)
        {
            List<TRStaticMesh> meshes = new List<TRStaticMesh>();

            foreach (TRStaticMesh mesh in GetStaticMeshes(level))
            {
                if (!mesh.NonCollidable)
                {
                    // Is it a fence/barrier? If so, we can allow it
                    int width = mesh.CollisionBox.MaxX - mesh.CollisionBox.MinX;
                    int depth = mesh.CollisionBox.MaxZ - mesh.CollisionBox.MinZ;
                    if ((width <= _fenceMinLimit && depth >= _fenceMaxLimit) || (width >= _fenceMaxLimit && depth <= _fenceMinLimit))
                    {
                        continue;
                    }
                    meshes.Add(mesh);
                }
            }

            return meshes;
        }

        private bool RoomHasCollisionalPortal(L level, short room)
        {
            foreach (TRRoomSector sector in GetRoomSectors(level, room))
            {
                if (sector.IsImpenetrable)
                {
                    continue;
                }

                // Vertical?
                if (sector.RoomAbove != _noRoom || sector.RoomBelow != _noRoom)
                {
                    return true;
                }

                // Horizontal?
                if (sector.FDIndex != 0)
                {
                    if (_floorData.Entries[sector.FDIndex].Any(e => e is FDPortalEntry))
                    {
                        return true;
                    }
                }
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

        private List<Location> GetValidLocations(L level)
        {
            List<Location> locations = new List<Location>();
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

                    // Default position adjustments for a flat sector
                    int dx = _halfSectorSize;
                    int dz = _halfSectorSize;
                    int dy = 0;
                    short angle = -1;

                    bool isTrianglePortal = false;

                    // Check the floor data, if there is any
                    if (sector.FDIndex != 0)
                    {
                        List<FDEntry> entries = _floorData.Entries[sector.FDIndex];
                        bool invalidFloorData = false;
                        foreach (FDEntry entry in entries)
                        {
                            if (entry is FDTriggerEntry trigger)
                            {
                                // Basic trigger checks - e.g. end level, underwater current
                                if (IsTriggerInvalid(trigger))
                                {
                                    invalidFloorData = true;
                                    break;
                                }
                            }
                            else if (entry is FDKillLaraEntry)
                            {
                                // For obvious reasons
                                invalidFloorData = true;
                                break;
                            }
                            else if (entry is FDSlantEntry slant && slant.Type == FDSlantEntryType.FloorSlant)
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
                            else if (entry is TR3TriangulationEntry triangulation && triangulation.IsFloorTriangulation)
                            {
                                Vector4? bestMidpoint = GetBestTriangleMidpoint(sector, triangulation, sectorIndex, GetRoomDepth(level, r), GetRoomYTop(level, r));
                                if (bestMidpoint.HasValue)
                                {
                                    dx = (int)(bestMidpoint.Value.X * _qrtSectorSize);
                                    dy = (int)(bestMidpoint.Value.Y * _qrtSectorSize);
                                    dz = (int)(bestMidpoint.Value.Z * _qrtSectorSize);
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
                        }

                        // Bail out if we don't like the look of the FD here
                        if (invalidFloorData)
                        {
                            continue;
                        }
                    }

                    // Final check for vertical portals - if a triangulation entry was detected and one
                    // of the triangles is a portal, we can allow it but if there are no triangles, this
                    // sector is in mid-air.
                    if (sector.RoomBelow != _noRoom && !isTrianglePortal)
                    {
                        continue;
                    }

                    // Make the location and adjust the positioning on this tile
                    Location location = CreateLocation(r, sector, sectorIndex, GetRoomDepth(level, r), GetRoomPosition(level, r));
                    location.X += dx;
                    location.Y += dy;
                    location.Z += dz;
                    location.Angle = angle;
                    locations.Add(location);
                }
            }

            return locations;
        }

        private bool IsSectorInvalid(TRRoomSector sector)
        {
            // Inside a wall, on a "normal" slope, or too near the ceiling
            return _excludedSectors.Contains(sector)
                || sector.IsImpenetrable
                || sector.IsSlipperySlope
                || ((sector.Floor - sector.Ceiling) < (CrawlspacesAllowed ? 2 : 4) && sector.RoomAbove == _noRoom);
        }

        private bool IsTriggerInvalid(FDTriggerEntry trigger)
        {
            // Any trigger types where we don't want items placed
            return trigger.TrigType == FDTrigType.Pickup
                || trigger.TrigActionList.Any
                (
                    a =>
                        a.TrigAction == FDTrigAction.UnderwaterCurrent
                     || a.TrigAction == FDTrigAction.EndLevel
                );
        }

        public Vector4? GetBestSlantMidpoint(FDSlantEntry slant)
        {
            if (Math.Abs(slant.XSlant) > 2 || Math.Abs(slant.ZSlant) > 2)
            {
                // This is a slippery slope
                return null;
            }

            List<sbyte> corners = new List<sbyte> { 0, 0, 0, 0 };
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

            dy = (dy * _qrtSectorSize) / 2; // Half-way down the slope
            return new Vector4(_halfSectorSize, dy, _halfSectorSize, angle);
        }

        // Returned vector contains x, y, z and angle adjustments for midpoint
        private Vector4? GetBestTriangleMidpoint(TRRoomSector sector, TR3TriangulationEntry triangulation, int sectorIndex, int roomDepth, int roomYTop)
        {
            int t0 = triangulation.TriData.C10;
            int t1 = triangulation.TriData.C00;
            int t2 = triangulation.TriData.C01;
            int t3 = triangulation.TriData.C11;

            List<byte> triangleCorners = new List<byte>
            {
                triangulation.TriData.C00,
                triangulation.TriData.C01,
                triangulation.TriData.C10,
                triangulation.TriData.C11
            };

            int max = triangleCorners.Max();
            List<int> corners = new List<int>
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

            int sectorXPos = sectorIndex / roomDepth * _fullSectorSize;
            int sectorZPos = sectorIndex % roomDepth * _fullSectorSize;

            FDFunctions func = (FDFunctions)triangulation.Setup.Function;
            switch (func)
            {
                case FDFunctions.FloorTriangulationNWSE_Solid:
                case FDFunctions.FloorTriangulationNWSE_SW:
                case FDFunctions.FloorTriangulationNWSE_NE:
                    triangle1 = new List<Vector3>
                    {
                        new Vector3(0, corners[0], 0),
                        new Vector3(0, corners[1], 4),
                        new Vector3(4, corners[2], 0)
                    };
                    triangle2 = new List<Vector3>
                    {
                        new Vector3(0, corners[1], 4),
                        new Vector3(4, corners[2], 0),
                        new Vector3(4, corners[3], 4)
                    };

                    triSum1 = (triangle1[0] + triangle1[1] + triangle1[2]) / 3;
                    triSum2 = (triangle2[0] + triangle2[1] + triangle2[2]) / 3;

                    x1 = (int)(sectorXPos + triSum1.X * _qrtSectorSize);
                    z1 = (int)(sectorZPos + triSum1.Z * _qrtSectorSize);

                    // Which quarter of the tile are we in?
                    dx1 = x1 & (_fullSectorSize - 1);
                    dz1 = z1 & (_fullSectorSize - 1);

                    // Is this the top triangle?
                    if (dx1 <= (_fullSectorSize - dz1))
                    {
                        xoff1 = t2 - t1;
                        zoff1 = t0 - t1;
                    }
                    else
                    {
                        xoff1 = t3 - t0;
                        zoff1 = t3 - t2;
                    }

                    x2 = (int)(sectorXPos + triSum2.X * _qrtSectorSize);
                    z2 = (int)(sectorZPos + triSum2.Z * _qrtSectorSize);
                    dx2 = x2 & (_fullSectorSize - 1);
                    dz2 = z2 & (_fullSectorSize - 1);

                    if (dx2 <= (_fullSectorSize - dz2))
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
                    if (sector.Floor * _qrtSectorSize == roomYTop)
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
                    if (Math.Abs(xoff1) < Math.Abs(xoff2) && Math.Abs(zoff1) < Math.Abs(zoff2) && func != FDFunctions.FloorTriangulationNWSE_SW)
                    {
                        xOffset = xoff1;
                        zOffset = zoff1;
                        bestMatch = new Vector4(triSum1.X, triSum1.Y, triSum1.Z, angle);
                    }
                    else if (func != FDFunctions.FloorTriangulationNWSE_NE)
                    {
                        xOffset = xoff2;
                        zOffset = zoff2;
                        bestMatch = new Vector4(triSum2.X, triSum2.Y, triSum2.Z, angle);
                    }

                    break;

                case FDFunctions.FloorTriangulationNESW_Solid:
                case FDFunctions.FloorTriangulationNESW_SE:
                case FDFunctions.FloorTriangulationNESW_NW:
                    triangle1 = new List<Vector3>
                    {
                        new Vector3(0, corners[0], 0),
                        new Vector3(4, corners[2], 0),
                        new Vector3(4, corners[3], 4)
                    };
                    triangle2 = new List<Vector3>
                    {
                        new Vector3(0, corners[0], 0),
                        new Vector3(0, corners[1], 4),
                        new Vector3(4, corners[3], 4)
                    };

                    triSum1 = (triangle1[0] + triangle1[1] + triangle1[2]) / 3;
                    triSum2 = (triangle2[0] + triangle2[1] + triangle2[2]) / 3;

                    x1 = (int)(sectorXPos + triSum1.X * _qrtSectorSize);
                    z1 = (int)(sectorZPos + triSum1.Z * _qrtSectorSize);
                    dx1 = x1 & (_fullSectorSize - 1);
                    dz1 = z1 & (_fullSectorSize - 1);

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

                    x2 = (int)(sectorXPos + triSum2.X * _qrtSectorSize);
                    z2 = (int)(sectorZPos + triSum2.Z * _qrtSectorSize);
                    dx2 = x2 & (_fullSectorSize - 1);
                    dz2 = z2 & (_fullSectorSize - 1);

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

                    if (sector.Floor * _qrtSectorSize == roomYTop)
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

                    if (Math.Abs(xoff1) < Math.Abs(xoff2) && Math.Abs(zoff1) < Math.Abs(zoff2) && func != FDFunctions.FloorTriangulationNESW_NW)
                    {
                        xOffset = xoff1;
                        zOffset = zoff1;
                        bestMatch = new Vector4(triSum1.X, triSum1.Y, triSum1.Z, angle);
                    }
                    else if (func != FDFunctions.FloorTriangulationNESW_SE)
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

        private Location CreateLocation(short roomIndex, TRRoomSector sector, int sectorIndex, ushort roomDepth, Vector2 roomPosition)
        {
            // Get the sector's position in its room
            int x = sectorIndex / roomDepth * _fullSectorSize;
            int z = sectorIndex % roomDepth * _fullSectorSize;
            int y = sector.Floor * _qrtSectorSize;

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

        protected abstract void ReadFloorData(L level);
        protected abstract TRRoomSector GetSector(Location location, L level);
        protected abstract List<TRRoomSector> GetRoomSectors(L level, int room);
        protected abstract List<TRStaticMesh> GetStaticMeshes(L level);
        protected abstract int GetRoomCount(L level);
        protected abstract short GetFlipMapRoom(L level, short room);
        protected abstract bool IsRoomValid(L level, short room);
        protected abstract Dictionary<ushort, List<Location>> GetRoomStaticMeshLocations(L level, short room);
        protected abstract ushort GetRoomDepth(L level, short room);
        protected abstract int GetRoomYTop(L level, short room);
        protected abstract Vector2 GetRoomPosition(L level, short room);
        public abstract bool CrawlspacesAllowed { get; }
    }
}