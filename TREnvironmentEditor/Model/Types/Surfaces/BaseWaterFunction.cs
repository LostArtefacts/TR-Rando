using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRLevelControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public abstract class BaseWaterFunction : BaseEMFunction, ITextureModifier
{
    public int[] RoomNumbers { get; set; }
    public ushort[] WaterTextures { get; set; }

    public void AddWaterSurface(TR1Room room, bool asCeiling, IEnumerable<int> adjacentRooms)
    {
        if (WaterTextures == null || WaterTextures.Length == 0)
        {
            // We may be flooding below ice, or water surfaces may already exist
            return;
        }

        for (int i = 0; i < room.Sectors.Count; i++)
        {
            TRRoomSector sector = room.Sectors[i];

            bool ceilingMatch = asCeiling && adjacentRooms.Contains(sector.RoomAbove);
            bool floorMatch = !asCeiling && adjacentRooms.Contains(sector.RoomBelow);

            if (ceilingMatch || floorMatch)
            {
                short x = (short)(i / room.NumZSectors * TRConsts.Step4);
                short z = (short)(i % room.NumZSectors * TRConsts.Step4);
                short y = (short)((ceilingMatch ? sector.Ceiling : sector.Floor) * TRConsts.Step1);

                List<ushort> vertIndices = new();

                List<TRVertex> defaultVerts = GetTileVertices(x, y, z, asCeiling);

                for (int k = 0; k < defaultVerts.Count; k++)
                {
                    TRVertex vert = defaultVerts[k];
                    int vi = room.RoomData.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert);
                    }
                    else
                    {
                        TR1RoomVertex exVert = room.RoomData.Vertices[vi];
                    }
                    vertIndices.Add((ushort)vi);
                }

                room.RoomData.Rectangles.Add(new()
                {
                    Texture = WaterTextures[0],
                    Vertices = vertIndices.ToArray()
                });
            }
        }
    }

    public void AddWaterSurface(TR2Room room, bool asCeiling, IEnumerable<int> adjacentRooms)
    {
        if (WaterTextures == null || WaterTextures.Length == 0)
        {
            // We may be flooding below ice, or water surfaces may already exist
            return;
        }

        for (int i = 0; i < room.Sectors.Count; i++)
        {
            TRRoomSector sector = room.Sectors[i];

            bool ceilingMatch = asCeiling && adjacentRooms.Contains(sector.RoomAbove);
            bool floorMatch = !asCeiling && adjacentRooms.Contains(sector.RoomBelow);

            if (ceilingMatch || floorMatch)
            {
                short x = (short)(i / room.NumZSectors * TRConsts.Step4);
                short z = (short)(i % room.NumZSectors * TRConsts.Step4);
                short y = (short)((ceilingMatch ? sector.Ceiling : sector.Floor) * TRConsts.Step1);

                List<ushort> vertIndices = new();

                List<TRVertex> defaultVerts = GetTileVertices(x, y, z, asCeiling);

                for (int k = 0; k < defaultVerts.Count; k++)
                {
                    TRVertex vert = defaultVerts[k];
                    int vi = room.RoomData.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert);
                    }
                    else
                    {
                        TR2RoomVertex exVert = room.RoomData.Vertices[vi];
                        exVert.Attributes = 32784; // Stop the shimmering
                    }
                    vertIndices.Add((ushort)vi);
                }

                room.RoomData.Rectangles.Add(new()
                {
                    Texture = WaterTextures[0],
                    Vertices = vertIndices.ToArray()
                });
            }
        }
    }

    public void AddWaterSurface(TR3Room room, bool asCeiling, IEnumerable<int> adjacentRooms, FDControl floorData)
    {
        if (WaterTextures == null || WaterTextures.Length == 0)
        {
            // We may be flooding below ice, or water surfaces may already exist
            return;
        }

        List<TR3RoomVertex> vertices = room.RoomData.Vertices.ToList();
        List<TRFace4> rectangles = room.RoomData.Rectangles.ToList();

        int count = 0;
        for (int i = 0; i < room.Sectors.Count; i++)
        {
            TRRoomSector sector = room.Sectors[i];

            bool ceilingMatch = asCeiling && adjacentRooms.Contains(sector.RoomAbove);
            bool floorMatch = !asCeiling && adjacentRooms.Contains(sector.RoomBelow);
            // Ignore triangles for now
            bool isTriangle = sector.FDIndex != 0 && floorData.Entries[sector.FDIndex].Any(e => e is TR3TriangulationEntry);

            if (!isTriangle && (ceilingMatch || floorMatch))
            {
                short x = (short)(i / room.NumZSectors * TRConsts.Step4);
                short z = (short)(i % room.NumZSectors * TRConsts.Step4);
                short y = (short)((ceilingMatch ? sector.Ceiling : sector.Floor) * TRConsts.Step1);

                List<ushort> vertIndices = new();

                List<TRVertex> defaultVerts = GetTileVertices(x, y, z, asCeiling);

                for (int k = 0; k < defaultVerts.Count; k++)
                {
                    TRVertex vert = defaultVerts[k];
                    int vi = vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert, useCaustics:true, useWaveMovement:true);
                    }
                    else
                    {
                        TR3RoomVertex exVert = room.RoomData.Vertices[vi];
                        //exVert.Attributes = 32784; // Stop the shimmering
                    }
                    vertIndices.Add((ushort)vi);
                }

                rectangles.Add(new TRFace4
                {
                    Texture = (ushort)(WaterTextures[count++ % WaterTextures.Length] | 0x8000), // Cycle through the textures and make them double-sided
                    Vertices = vertIndices.ToArray()
                });
            }
        }

        room.RoomData.Rectangles = rectangles.ToArray();
        room.RoomData.NumRectangles = (short)rectangles.Count;
    }

    public void RemoveWaterSurface(TR1Room room)
    {
        RemoveWaterSurfaces(room.RoomData.Rectangles);
    }

    public void RemoveWaterSurface(TR2Room room)
    {
        RemoveWaterSurfaces(room.RoomData.Rectangles);
    }

    public void RemoveWaterSurface(TR3Room room)
    {
        List<TRFace4> rs = room.RoomData.Rectangles.ToList();
        RemoveWaterSurfaces(rs);

        room.RoomData.Rectangles = rs.ToArray();
        room.RoomData.NumRectangles = (short)rs.Count;
    }

    public void RemoveWaterSurfaces(List<TRFace4> faces)
    {
        for (int i = faces.Count - 1; i >= 0; i--)
        {
            TRFace4 face = faces[i];
            if (WaterTextures.Contains((ushort)(face.Texture & 0x0fff)))
            {
                faces.RemoveAt(i);
            }
        }
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        if (WaterTextures != null)
        {
            List<ushort> textures = WaterTextures.ToList();
            foreach (ushort texture in indexMap.Keys)
            {
                int i = textures.IndexOf(texture);
                if (i != -1)
                {
                    textures.Insert(i, indexMap[texture]);
                    textures.Remove(texture);
                }
            }
            WaterTextures = textures.ToArray();
        }
    }
}
