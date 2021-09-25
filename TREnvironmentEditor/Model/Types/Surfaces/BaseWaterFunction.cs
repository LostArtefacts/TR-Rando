using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public abstract class BaseWaterFunction : BaseEMFunction, ITextureModifier
    {
        public int[] RoomNumbers { get; set; }
        public ushort[] WaterTextures { get; set; }
        
        public void AddWaterSurface(TR2Level level, TR2Room room, bool asCeiling, IEnumerable<int> adjacentRooms)
        {
            if (WaterTextures == null || WaterTextures.Length == 0)
            {
                // We may be flooding below ice, or water surfaces may already exist
                return;
            }

            List<TR2RoomVertex> vertices = room.RoomData.Vertices.ToList();
            List<TRFace4> rectangles = room.RoomData.Rectangles.ToList();

            for (int i = 0; i < room.SectorList.Length; i++)
            {
                TRRoomSector sector = room.SectorList[i];

                bool ceilingMatch = asCeiling && adjacentRooms.Contains(sector.RoomAbove);
                bool floorMatch = !asCeiling && adjacentRooms.Contains(sector.RoomBelow);

                if (ceilingMatch || floorMatch)
                {
                    short x = (short)(i / room.NumZSectors * SectorSize);
                    short z = (short)(i % room.NumZSectors * SectorSize);
                    short y = (short)((ceilingMatch ? sector.Ceiling : sector.Floor) * ClickSize);

                    List<ushort> vertIndices = new List<ushort>();

                    List<TRVertex> defaultVerts = GetTileVertices(x, y, z, asCeiling);

                    for (int k = 0; k < defaultVerts.Count; k++)
                    {
                        TRVertex vert = defaultVerts[k];
                        int vi = vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
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

                    rectangles.Add(new TRFace4
                    {
                        Texture = WaterTextures[0],
                        Vertices = vertIndices.ToArray()
                    });
                }
            }

            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }

        public void RemoveWaterSurface(TR2Room room)
        {
            List<TRFace4> rs = room.RoomData.Rectangles.ToList();
            foreach (TRFace4 r in room.RoomData.Rectangles)
            {
                if (WaterTextures.Contains(r.Texture))
                {
                    rs.Remove(r);
                }
            }

            room.RoomData.Rectangles = rs.ToArray();
            room.RoomData.NumRectangles = (short)rs.Count;

            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
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
}