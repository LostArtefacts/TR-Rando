﻿using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRRandomizerCore.Utilities;

public static class FaceUtilities
{
    public static List<TRFace4> GetTriggerFaces(TR1Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        List<TRFace4> faces = new();
        foreach (TRRoom room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(floorData, triggerTypes, includeDeathTiles, room.Sectors, room.NumZSectors, room.RoomData.Rectangles, v =>
            {
                return room.RoomData.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static List<TRFace4> GetTriggerFaces(TR2Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        List<TRFace4> faces = new();
        foreach (TR2Room room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(floorData, triggerTypes, includeDeathTiles, room.SectorList, room.NumZSectors, room.RoomData.Rectangles, v =>
            {
                return room.RoomData.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static List<TRFace4> GetTriggerFaces(TR3Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        List<TRFace4> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(floorData, triggerTypes, includeDeathTiles, room.Sectors, room.NumZSectors, room.RoomData.Rectangles, v =>
            {
                return room.RoomData.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static Dictionary<TRFace4, List<TRVertex>> GetClimbableFaces(TR2Level level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        Dictionary<TRFace4, List<TRVertex>> faces = new();
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
        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        Dictionary<TRFace4, List<TRVertex>> faces = new();
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

    private static List<TRFace4> ScanTriggerFaces
        (FDControl floorData, List<FDTrigType> triggerMatches, bool includeDeathTiles, TRRoomSector[] sectors, ushort roomDepth, TRFace4[] roomFaces, Func<ushort, TRVertex> vertexAction)
    {
        List<TRFace4> faces = new();
        for (int i = 0; i < sectors.Length; i++)
        {
            TRRoomSector sector = sectors[i];
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            if ((entries.Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger && triggerMatches.Contains(trigger.TrigType))
                || (includeDeathTiles && entries.Any(e => e is FDKillLaraEntry)))
            {
                short x = (short)(i / roomDepth * TRConsts.Step4);
                short z = (short)(i % roomDepth * TRConsts.Step4);
                short y = (short)(sector.Floor * TRConsts.Step1);

                List<TRVertex> vertMatches = GetFloorOrCeilingVerticesToMatch(x, z);

                foreach (TRFace4 face in roomFaces)
                {
                    List<TRVertex> faceVertices = new();
                    foreach (ushort v in face.Vertices)
                    {
                        faceVertices.Add(vertexAction.Invoke(v));
                    }

                    if (IsFloorMatch(vertMatches, faceVertices, y))
                    {
                        faces.Add(face);
                    }
                }
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

        entry ??= floorData.Entries[sector.FDIndex].Find(e => e is FDClimbEntry);

        if (entry is FDClimbEntry climbEntry)
        {
            int sectorIndex = room.SectorList.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);

            List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

            foreach (TRFace4 face in room.RoomData.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                }

                if (IsWallMatch(vertMatches, faceVertices))
                {
                    faces.Add(face, faceVertices);
                }
            }

            if (sector.RoomAbove != TRConsts.NoRoom)
            {
                TR2Room roomAbove = level.Rooms[sector.RoomAbove];
                int wx = room.Info.X + x;
                int wz = room.Info.Z + z;
                int wy = (sector.Ceiling - 1) * TRConsts.Step1;
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

        entry ??= floorData.Entries[sector.FDIndex].Find(e => e is FDClimbEntry);

        if (entry is FDClimbEntry climbEntry)
        {
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);

            List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

            foreach (TRFace4 face in room.RoomData.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                }

                if (IsWallMatch(vertMatches, faceVertices))
                {
                    faces.Add(face, faceVertices);
                }
            }

            if (sector.RoomAbove != TRConsts.NoRoom)
            {
                TR3Room roomAbove = level.Rooms[sector.RoomAbove];
                int wx = room.Info.X + x;
                int wz = room.Info.Z + z;
                int wy = (sector.Ceiling - 1) * TRConsts.Step1;
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

        entry ??= floorData.Entries[sector.FDIndex].Find(e => e is TR3MonkeySwingEntry);

        if (entry is TR3MonkeySwingEntry monkeyEntry)
        {
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);
            short y = (short)(sector.Floor * TRConsts.Step1);

            List<TRVertex> vertMatches = GetFloorOrCeilingVerticesToMatch(x, z);

            foreach (TRFace4 face in room.RoomData.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.RoomData.Vertices[v].Vertex);
                }

                if (IsCeilingMatch(vertMatches, faceVertices, y))
                {
                    faces.Add(face, faceVertices);
                }
            }

            if (sector.RoomAbove != TRConsts.NoRoom)
            {
                TR3Room roomAbove = level.Rooms[sector.RoomAbove];
                int wx = room.Info.X + x;
                int wz = room.Info.Z + z;
                int wy = (sector.Ceiling - 1) * TRConsts.Step1;
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
        List<TRVertex> vertMatches = new();
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
                Z = (short)(z + TRConsts.Step4)
            });
        }
        if (climbEntry.IsPositiveX)
        {
            vertMatches.Add(new TRVertex
            {
                X = (short)(x + TRConsts.Step4),
                Z = z
            });
            vertMatches.Add(new TRVertex
            {
                X = (short)(x + TRConsts.Step4),
                Z = (short)(z + TRConsts.Step4)
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
                X = (short)(x + TRConsts.Step4),
                Z = z
            });
        }
        if (climbEntry.IsPositiveZ)
        {
            vertMatches.Add(new TRVertex
            {
                X = x,
                Z = (short)(z + TRConsts.Step4)
            });
            vertMatches.Add(new TRVertex
            {
                X = (short)(x + TRConsts.Step4),
                Z = (short)(z + TRConsts.Step4)
            });
        }

        return vertMatches;
    }

    private static List<TRVertex> GetFloorOrCeilingVerticesToMatch(short x, short z)
    {
        List<TRVertex> vertMatches = new()
        {
            new TRVertex
            {
                X = x,
                Z = z
            },
            new TRVertex
            {
                X = (short)(x + TRConsts.Step4),
                Z = z
            },
            new TRVertex
            {
                X = x,
                Z = (short)(z + TRConsts.Step4)
            },
            new TRVertex
            {
                X = (short)(x + TRConsts.Step4),
                Z = (short)(z + TRConsts.Step4)
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

    public static bool IsFloorMatch(List<TRVertex> sectorVertices, List<TRVertex> faceVertices, short floorY)
    {
        // Is this a floor and is every vertex in the sector check part of this face?
        return faceVertices.All(v => v.Y >= floorY) &&
            sectorVertices.All(v1 => faceVertices.Any(v2 => v1.X == v2.X && v1.Z == v2.Z));
    }
}
