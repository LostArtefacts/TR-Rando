using System.Numerics;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRRandomizerCore.Utilities;

public static class FaceUtilities
{
    public static List<TRFace> GetTriggerFaces(TR1Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        List<TRFace> faces = new();
        foreach (TR1Room room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(level.FloorData, triggerTypes, includeDeathTiles, room.Sectors, room.NumZSectors, room.Mesh.Rectangles, v =>
            {
                return room.Mesh.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static List<TRFace> GetTriggerFaces(TR2Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        List<TRFace> faces = new();
        foreach (TR2Room room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(level.FloorData, triggerTypes, includeDeathTiles, room.Sectors, room.NumZSectors, room.Mesh.Rectangles, v =>
            {
                return room.Mesh.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static List<TRFace> GetTriggerFaces(TR3Level level, List<FDTrigType> triggerTypes, bool includeDeathTiles)
    {
        List<TRFace> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            faces.AddRange(ScanTriggerFaces(level.FloorData, triggerTypes, includeDeathTiles, room.Sectors, room.NumZSectors, room.Mesh.Rectangles, v =>
            {
                return room.Mesh.Vertices[v].Vertex;
            }));
        }

        return faces;
    }

    public static Dictionary<TRFace, List<TRVertex>> GetClimbableFaces(TR2Level level)
    {
        Dictionary<TRFace, List<TRVertex>> faces = new();
        foreach (TR2Room room in level.Rooms)
        {
            foreach (TRRoomSector sector in room.Sectors)
            {
                ScanTR2SectorLadderFaces(faces, level, room, sector);
            }
        }

        return faces;
    }

    public static Dictionary<TRFace, List<TRVertex>> GetClimbableFaces(TR3Level level)
    {
        Dictionary<TRFace, List<TRVertex>> faces = new();
        foreach (TR3Room room in level.Rooms)
        {
            foreach (TRRoomSector sector in room.Sectors)
            {
                ScanTR3SectorLadderFaces(faces, level, room, sector);
                ScanTR3SectorMonkeyFaces(faces, level, room, sector);
            }
        }

        return faces;
    }

    private static List<TRFace> ScanTriggerFaces
        (FDControl floorData, List<FDTrigType> triggerMatches, bool includeDeathTiles, List<TRRoomSector> sectors, ushort roomDepth, List<TRFace> roomFaces, Func<ushort, TRVertex> vertexAction)
    {
        List<TRFace> faces = new();
        for (int i = 0; i < sectors.Count; i++)
        {
            TRRoomSector sector = sectors[i];
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = floorData[sector.FDIndex];
            if ((entries.Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger && triggerMatches.Contains(trigger.TrigType))
                || (includeDeathTiles && entries.Any(e => e is FDKillLaraEntry)))
            {
                short x = (short)(i / roomDepth * TRConsts.Step4);
                short z = (short)(i % roomDepth * TRConsts.Step4);
                short y = (short)(sector.Floor * TRConsts.Step1);

                List<TRVertex> vertMatches = GetFloorOrCeilingVerticesToMatch(x, z);

                foreach (TRFace face in roomFaces)
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

    private static void ScanTR2SectorLadderFaces(Dictionary<TRFace, List<TRVertex>> faces, TR2Level level, TR2Room room, TRRoomSector sector, FDEntry entry = null)
    {
        if (entry == null && sector.FDIndex == 0)
        {
            return;
        }

        entry ??= level.FloorData[sector.FDIndex].Find(e => e is FDClimbEntry);

        if (entry is FDClimbEntry climbEntry)
        {
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);

            List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

            foreach (TRFace face in room.Mesh.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.Mesh.Vertices[v].Vertex);
                }

                if (IsWallMatch(vertMatches, faceVertices))
                {
                    faces.Add(face, [.. room.Mesh.Vertices.Select(v => v.Vertex)]);
                }
            }

            if (sector.RoomAbove != TRConsts.NoRoom)
            {
                TR2Room roomAbove = level.Rooms[sector.RoomAbove];
                int wx = room.Info.X + x;
                int wz = room.Info.Z + z;
                int wy = (sector.Ceiling - 1) * TRConsts.Step1;
                TRRoomSector sectorAbove = level.GetRoomSector(wx, wy, wz, sector.RoomAbove);
                if (sector != sectorAbove)
                {
                    ScanTR2SectorLadderFaces(faces, level, roomAbove, sectorAbove, entry);
                }
            }
        }
    }

    private static void ScanTR3SectorLadderFaces(Dictionary<TRFace, List<TRVertex>> faces, TR3Level level, TR3Room room, TRRoomSector sector, FDEntry entry = null)
    {
        if (entry == null && sector.FDIndex == 0)
        {
            return;
        }

        entry ??= level.FloorData[sector.FDIndex].Find(e => e is FDClimbEntry);

        if (entry is FDClimbEntry climbEntry)
        {
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);

            List<TRVertex> vertMatches = GetVerticesToMatch(climbEntry, x, z);

            foreach (TRFace face in room.Mesh.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.Mesh.Vertices[v].Vertex);
                }

                if (IsWallMatch(vertMatches, faceVertices))
                {
                    faces.Add(face, [.. room.Mesh.Vertices.Select(v => v.Vertex)]);
                }
            }

            if (sector.RoomAbove != TRConsts.NoRoom)
            {
                TR3Room roomAbove = level.Rooms[sector.RoomAbove];
                int wx = room.Info.X + x;
                int wz = room.Info.Z + z;
                int wy = (sector.Ceiling - 1) * TRConsts.Step1;
                TRRoomSector sectorAbove = level.GetRoomSector(wx, wy, wz, sector.RoomAbove);
                if (sector != sectorAbove)
                {
                    ScanTR3SectorLadderFaces(faces, level, roomAbove, sectorAbove, entry);
                }
            }
        }
    }

    private static void ScanTR3SectorMonkeyFaces(Dictionary<TRFace, List<TRVertex>> faces, TR3Level level, TR3Room room, TRRoomSector sector, FDEntry entry = null)
    {
        if (entry == null && sector.FDIndex == 0)
        {
            return;
        }

        entry ??= level.FloorData[sector.FDIndex].Find(e => e is FDMonkeySwingEntry);

        if (entry is FDMonkeySwingEntry monkeyEntry)
        {
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);
            short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
            short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);
            short y = (short)(sector.Floor * TRConsts.Step1);

            List<TRVertex> vertMatches = GetFloorOrCeilingVerticesToMatch(x, z);

            foreach (TRFace face in room.Mesh.Rectangles)
            {
                if (faces.ContainsKey(face))
                {
                    continue;
                }

                List<TRVertex> faceVertices = new();
                foreach (ushort v in face.Vertices)
                {
                    faceVertices.Add(room.Mesh.Vertices[v].Vertex);
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
                TRRoomSector sectorAbove = level.GetRoomSector(wx, wy, wz, sector.RoomAbove);
                if (sector != sectorAbove)
                {
                    ScanTR3SectorMonkeyFaces(faces, level, roomAbove, sectorAbove, entry);
                }
            }
        }
    }

    private static List<TRVertex> GetVerticesToMatch(FDClimbEntry climbEntry, short x, short z)
    {
        List<TRVertex> vertMatches = new();
        if (climbEntry.IsNegativeX)
        {
            vertMatches.Add(new()
            {
                X = x,
                Z = z
            });
            vertMatches.Add(new()
            {
                X = x,
                Z = (short)(z + TRConsts.Step4)
            });
        }
        if (climbEntry.IsPositiveX)
        {
            vertMatches.Add(new()
            {
                X = (short)(x + TRConsts.Step4),
                Z = z
            });
            vertMatches.Add(new()
            {
                X = (short)(x + TRConsts.Step4),
                Z = (short)(z + TRConsts.Step4)
            });
        }
        if (climbEntry.IsNegativeZ)
        {
            vertMatches.Add(new()
            {
                X = x,
                Z = z
            });
            vertMatches.Add(new()
            {
                X = (short)(x + TRConsts.Step4),
                Z = z
            });
        }
        if (climbEntry.IsPositiveZ)
        {
            vertMatches.Add(new()
            {
                X = x,
                Z = (short)(z + TRConsts.Step4)
            });
            vertMatches.Add(new()
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
            new()
            {
                X = x,
                Z = z
            },
            new()
            {
                X = (short)(x + TRConsts.Step4),
                Z = z
            },
            new()
            {
                X = x,
                Z = (short)(z + TRConsts.Step4)
            },
            new()
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

    public static void NormaliseWallQuad(TRFace face, List<TRVertex> allVertices)
    {
        if (face.Type != TRFaceType.Rectangle)
        {
            return;
        }

        var vertices = face.Vertices.Select(v => allVertices[v]).ToList();
        var normal = GetNormal(vertices);
        if (normal.Y != 0)
        {
            return;
        }

        var sortedByY = vertices.OrderBy(v => v.Y).ToList();
        var top = sortedByY.Take(2).ToList();
        var bottom = sortedByY.Skip(2).ToList();
        
        if (normal.X != 0)
        {
            top = [.. top.OrderBy(v => v.Z)];
            bottom = [.. bottom.OrderBy(v => v.Z)];
            if (normal.X > 0)
            {
                top.Reverse();
                bottom.Reverse();
            }
        }
        else
        {
            top = [.. top.OrderBy(v => v.X)];
            bottom = [.. bottom.OrderBy(v => v.X)];
            if (normal.Z < 0)
            {
                top.Reverse();
                bottom.Reverse();
            }
        }

        face.Vertices[0] = (ushort)allVertices.IndexOf(top[0]);
        face.Vertices[1] = (ushort)allVertices.IndexOf(top[1]);
        face.Vertices[2] = (ushort)allVertices.IndexOf(bottom[1]);
        face.Vertices[3] = (ushort)allVertices.IndexOf(bottom[0]);
    }

    public static Vector3 GetNormal(List<TRVertex> vertices)
    {
        if (vertices.Count < 3)
        {
            throw new Exception();
        }

        var v0 = new Vector3(vertices[0].X, vertices[0].Y, vertices[0].Z);
        var v1 = new Vector3(vertices[1].X, vertices[1].Y, vertices[1].Z);
        var v2 = new Vector3(vertices[2].X, vertices[2].Y, vertices[2].Z);

        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;

        return Vector3.Normalize(Vector3.Cross(edge1, edge2));
    }
}
