using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TRRandomizerCore.Utilities
{
    public static class LadderUtilities
    {
        private static readonly byte _noRoom = 255;
        private static readonly int _fullSectorSize = 1024;
        private static readonly int _qrtSectorSize = 256;

        public static Dictionary<TRFace4, List<TRVertex>> GetClimbableFaces(TR2Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            Dictionary<TRFace4, List<TRVertex>> faces = new Dictionary<TRFace4, List<TRVertex>>();
            foreach (TR2Room room in level.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    ScanTR2SectorLadderFaces(faces, level, floorData, room, sector);
                }
            }

            return faces;
        }

        public static Dictionary<TRFace4, List<TRVertex>> GetClimbableFaces(TR3Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            Dictionary<TRFace4, List<TRVertex>> faces = new Dictionary<TRFace4, List<TRVertex>>();
            foreach (TR3Room room in level.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    ScanTR3SectorLadderFaces(faces, level, floorData, room, sector);
                    ScanTR3SectorMonkeyFaces(faces, level, floorData, room, sector);
                }
            }

            return faces;
        }

        private static void ScanTR2SectorLadderFaces(Dictionary<TRFace4, List<TRVertex>> faces, TR2Level level, FDControl floorData, TR2Room room, TRRoomSector sector, FDEntry entry = null)
        {
            if (entry == null && sector.FDIndex == 0)
            {
                return;
            }

            if (entry == null)
            {
                entry = floorData.Entries[sector.FDIndex].Find(e => e is FDClimbEntry);
            }

            if (entry is FDClimbEntry climbEntry)
            {
                int sectorIndex = room.SectorList.ToList().IndexOf(sector);
                short x = (short)(sectorIndex / room.NumZSectors * _fullSectorSize);
                short z = (short)(sectorIndex % room.NumZSectors * _fullSectorSize);

                List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

                foreach (TRFace4 face in room.RoomData.Rectangles)
                {
                    if (faces.ContainsKey(face))
                    {
                        continue;
                    }

                    List<TRVertex> faceVertices = new List<TRVertex>();
                    foreach (ushort v in face.Vertices)
                    {
                        faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                    }

                    if (IsWallMatch(vertMatches, faceVertices))
                    {
                        faces.Add(face, faceVertices);
                    }
                }

                if (sector.RoomAbove != _noRoom)
                {
                    TR2Room roomAbove = level.Rooms[sector.RoomAbove];
                    int wx = room.Info.X + x;
                    int wz = room.Info.Z + z;
                    int wy = (sector.Ceiling - 1) * _qrtSectorSize;
                    TRRoomSector sectorAbove = FDUtilities.GetRoomSector(wx, wy, wz, sector.RoomAbove, level, floorData);
                    if (sector != sectorAbove)
                    {
                        ScanTR2SectorLadderFaces(faces, level, floorData, roomAbove, sectorAbove, entry);
                    }
                }
            }
        }

        private static void ScanTR3SectorLadderFaces(Dictionary<TRFace4, List<TRVertex>> faces, TR3Level level, FDControl floorData, TR3Room room, TRRoomSector sector, FDEntry entry = null)
        {
            if (entry == null && sector.FDIndex == 0)
            {
                return;
            }

            if (entry == null)
            {
                entry = floorData.Entries[sector.FDIndex].Find(e => e is FDClimbEntry);
            }

            if (entry is FDClimbEntry climbEntry)
            {
                int sectorIndex = room.Sectors.ToList().IndexOf(sector);
                short x = (short)(sectorIndex / room.NumZSectors * _fullSectorSize);
                short z = (short)(sectorIndex % room.NumZSectors * _fullSectorSize);

                List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

                foreach (TRFace4 face in room.RoomData.Rectangles)
                {
                    if (faces.ContainsKey(face))
                    {
                        continue;
                    }

                    List<TRVertex> faceVertices = new List<TRVertex>();
                    foreach (ushort v in face.Vertices)
                    {
                        faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                    }

                    if (IsWallMatch(vertMatches, faceVertices))
                    {
                        faces.Add(face, faceVertices);
                    }
                }

                if (sector.RoomAbove != _noRoom)
                {
                    TR3Room roomAbove = level.Rooms[sector.RoomAbove];
                    int wx = room.Info.X + x;
                    int wz = room.Info.Z + z;
                    int wy = (sector.Ceiling - 1) * _qrtSectorSize;
                    TRRoomSector sectorAbove = FDUtilities.GetRoomSector(wx, wy, wz, sector.RoomAbove, level, floorData);
                    if (sector != sectorAbove)
                    {
                        ScanTR3SectorLadderFaces(faces, level, floorData, roomAbove, sectorAbove, entry);
                    }
                }
            }
        }

        private static void ScanTR3SectorMonkeyFaces(Dictionary<TRFace4, List<TRVertex>> faces, TR3Level level, FDControl floorData, TR3Room room, TRRoomSector sector, FDEntry entry = null)
        {
            if (entry == null && sector.FDIndex == 0)
            {
                return;
            }

            if (entry == null)
            {
                entry = floorData.Entries[sector.FDIndex].Find(e => e is TR3MonkeySwingEntry);
            }

            if (entry is TR3MonkeySwingEntry monkeyEntry)
            {
                int sectorIndex = room.Sectors.ToList().IndexOf(sector);
                short x = (short)(sectorIndex / room.NumZSectors * _fullSectorSize);
                short z = (short)(sectorIndex % room.NumZSectors * _fullSectorSize);
                short y = (short)(sector.Floor * _qrtSectorSize);

                List<TRVertex> vertMatches = GetVerticesToMatch(monkeyEntry, x, z);

                foreach (TRFace4 face in room.RoomData.Rectangles)
                {
                    if (faces.ContainsKey(face))
                    {
                        continue;
                    }

                    List<TRVertex> faceVertices = new List<TRVertex>();
                    foreach (ushort v in face.Vertices)
                    {
                        faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                    }

                    if (IsCeilingMatch(vertMatches, faceVertices, y))
                    {
                        faces.Add(face, faceVertices);
                    }
                }

                if (sector.RoomAbove != _noRoom)
                {
                    TR3Room roomAbove = level.Rooms[sector.RoomAbove];
                    int wx = room.Info.X + x;
                    int wz = room.Info.Z + z;
                    int wy = (sector.Ceiling - 1) * _qrtSectorSize;
                    TRRoomSector sectorAbove = FDUtilities.GetRoomSector(wx, wy, wz, sector.RoomAbove, level, floorData);
                    if (sector != sectorAbove)
                    {
                        ScanTR3SectorMonkeyFaces(faces, level, floorData, roomAbove, sectorAbove, entry);
                    }
                }
            }
        }

        private static List<TRVertex> GetVerticesToMatch(FDClimbEntry climbEntry, short x, short z)
        {
            List<TRVertex> vertMatches = new List<TRVertex>();
            if (climbEntry.IsNegativeX)
            {
                vertMatches.Add(new TRVertex
                {
                    X = x,
                    Z = z
                });
                vertMatches.Add(new TRVertex
                {
                    X = x,
                    Z = (short)(z + _fullSectorSize)
                });
            }
            if (climbEntry.IsPositiveX)
            {
                vertMatches.Add(new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = z
                });
                vertMatches.Add(new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = (short)(z + _fullSectorSize)
                });
            }
            if (climbEntry.IsNegativeZ)
            {
                vertMatches.Add(new TRVertex
                {
                    X = x,
                    Z = z
                });
                vertMatches.Add(new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = z
                });
            }
            if (climbEntry.IsPositiveZ)
            {
                vertMatches.Add(new TRVertex
                {
                    X = x,
                    Z = (short)(z + _fullSectorSize)
                });
                vertMatches.Add(new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = (short)(z + _fullSectorSize)
                });
            }

            return vertMatches;
        }

        private static List<TRVertex> GetVerticesToMatch(TR3MonkeySwingEntry monkeyEntry, short x, short z)
        {
            List<TRVertex> vertMatches = new List<TRVertex>
            {
                new TRVertex
                {
                    X = x,
                    Z = z
                },
                new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = z
                },
                new TRVertex
                {
                    X = x,
                    Z = (short)(z + _fullSectorSize)
                },
                new TRVertex
                {
                    X = (short)(x + _fullSectorSize),
                    Z = (short)(z + _fullSectorSize)
                }
            };            

            return vertMatches;
        }

        public static bool IsWallMatch(List<TRVertex> sectorVertices, List<TRVertex> faceVertices)
        {
            // Is this a wall and does every vertex's x and z value match the sector?
            return
            (
                faceVertices.All(v => v.X == faceVertices[0].X) ||
                faceVertices.All(v => v.Z == faceVertices[0].Z)
            )
            && faceVertices.All(v1 => sectorVertices.Any(v2 => v1.X == v2.X && v1.Z == v2.Z));
        }

        public static bool IsCeilingMatch(List<TRVertex> sectorVertices, List<TRVertex> faceVertices, short floorY)
        {
            // Is this a ceiling and is every vertex in the sector check part of this face?
            return faceVertices.All(v => v.Y < floorY) &&
                sectorVertices.All(v1 => faceVertices.Any(v2 => v1.X == v2.X && v1.Z == v2.Z));
        }
    }
}