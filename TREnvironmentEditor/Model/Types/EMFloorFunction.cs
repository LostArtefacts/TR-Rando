using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMFloorFunction : BaseEMFunction
    {
        public EMLocation Location { get; set; }
        public sbyte Clicks { get; set; }
        public ushort FloorTexture { get; set; }
        public ushort SideTexture { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            // Find the vertices of the current floor for the tile at the given location, create 4 additional 
            // vertices on top and make new TRFace4 entries for the sides so the platform isn't floating.
            //
            // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with
            // TODO: check for item locations

            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            TR2Room room = level.Rooms[Location.Room];
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)Location.Room, level, fdc);
            int sectorIndex = room.SectorList.ToList().IndexOf(sector);

            // Find the current vertices for this tile
            // TODO This is going to be common for the likes of applying/removing
            // animated water textures, so it should be moved elsewhere.
            short x = (short)(sectorIndex / room.NumZSectors * SectorSize);
            short z = (short)(sectorIndex % room.NumZSectors * SectorSize);
            short y = (short)(sector.Floor * ClickSize);

            List<TR2RoomVertex> vertices = room.RoomData.Vertices.ToList();
            List<ushort> oldVertIndices = new List<ushort>();

            List<TRVertex> defVerts = GetTileVertices(x, y, z, false);

            for (int i = 0; i < defVerts.Count; i++)
            {
                TRVertex vert = defVerts[i];
                int vi = vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
                if (vi != -1)
                {
                    oldVertIndices.Add((ushort)vi);
                }
            }

            // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
            // those and we need the originals for the new sides to this platform.
            List<ushort> newVertIndices = new List<ushort>();
            foreach (ushort vert in oldVertIndices)
            {
                TRVertex oldVert = vertices[vert].Vertex;
                TRVertex newVertex = new TRVertex
                {
                    X = oldVert.X,
                    Y = (short)(oldVert.Y + (Clicks * ClickSize)),
                    Z = oldVert.Z
                };
                newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex));
            }

            // Get the tile face that matches the vertex list
            List<TRFace4> rectangles = room.RoomData.Rectangles.ToList();
            TRFace4 floorFace = rectangles.Find(r => r.Vertices.ToList().All(oldVertIndices.Contains));
            if (floorFace != null)
            {
                floorFace.Vertices = newVertIndices.ToArray();
                floorFace.Texture = FloorTexture;
            }

            // Make faces for the new platform's sides
            for (int i = 0; i < 4; i++)
            {
                int j = i == 3 ? 0 : (i + 1);
                rectangles.Add(new TRFace4
                {
                    Texture = SideTexture,
                    Vertices = new ushort[]
                    {
                        newVertIndices[j],
                        newVertIndices[i],
                        oldVertIndices[i],
                        oldVertIndices[j]
                    }
                });
            }

            // Save the new faces
            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Now shift the actual sector info up.
            sector.Floor += Clicks;
            level.Boxes[sector.BoxIndex].TrueFloor = (short)(sector.Floor * ClickSize);

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }
    }
}