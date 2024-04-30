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
                    int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert);
                    }
                    else
                    {
                        TR1RoomVertex exVert = room.Mesh.Vertices[vi];
                    }
                    vertIndices.Add((ushort)vi);
                }

                room.Mesh.Rectangles.Add(new()
                {
                    Texture = WaterTextures[0],
                    Vertices = vertIndices
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
                    int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert);
                    }
                    else
                    {
                        TR2RoomVertex exVert = room.Mesh.Vertices[vi];
                        exVert.Attributes = 32784; // Stop the shimmering
                    }
                    vertIndices.Add((ushort)vi);
                }

                room.Mesh.Rectangles.Add(new()
                {
                    Texture = WaterTextures[0],
                    Vertices = vertIndices
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
                    int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                    if (vi == -1)
                    {
                        vi = CreateRoomVertex(room, vert, useCaustics:true, useWaveMovement:true);
                    }
                    vertIndices.Add((ushort)vi);
                }

                room.Mesh.Rectangles.Add(new()
                {
                    Texture = WaterTextures[count++ % WaterTextures.Length], // Cycle through the textures and make them double-sided
                    DoubleSided = true,
                    Vertices = vertIndices
                });
            }
        }
    }

    public void RemoveWaterSurface(TR1Room room)
    {
        RemoveWaterSurfaces(room.Mesh.Rectangles);
    }

    public void RemoveWaterSurface(TR2Room room)
    {
        RemoveWaterSurfaces(room.Mesh.Rectangles);
    }

    public void RemoveWaterSurface(TR3Room room)
    {
        RemoveWaterSurfaces(room.Mesh.Rectangles);
    }

    public void RemoveWaterSurfaces(List<TRFace> faces)
    {
        for (int i = faces.Count - 1; i >= 0; i--)
        {
            TRFace face = faces[i];
            if (WaterTextures.Contains(face.Texture))
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
