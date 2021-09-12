using System;
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
        public bool RetainOriginalFloor { get; set; }

        public EMFloorFunction()
        {
            FloorTexture = ushort.MaxValue;
            SideTexture = ushort.MaxValue;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            if (Clicks == 0)
            {
                return;
            }
            
            MoveFloor(level);
        }

        private void MoveFloor(TR2Level level)
        {
            // Find the vertices of the current floor for the tile at the given location, create 4 additional 
            // vertices on top and make new TRFace4 entries for the sides so the platform isn't floating.
            // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with.

            int clickChange = Clicks * ClickSize;

            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            TR2Room room = level.Rooms[Location.Room];
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, Location.Room, level, fdc);
            int sectorIndex = room.SectorList.ToList().IndexOf(sector);

            // Find the current vertices for this tile
            short x = (short)(sectorIndex / room.NumZSectors * SectorSize);
            short z = (short)(sectorIndex % room.NumZSectors * SectorSize);
            short y = (short)(sector.Floor * ClickSize);

            List<TR2RoomVertex> vertices = room.RoomData.Vertices.ToList();
            List<ushort> oldVertIndices = new List<ushort>();

            List<TRVertex> defVerts = GetTileVertices(x, y, z, false);
            // Check the Y vals are unanimous because we currently only support raising/lowering flat surfaces
            if (!defVerts.All(v => v.Y == defVerts[0].Y))
            {
                throw new NotImplementedException("Floor manipulation is limited to flat surfaces only.");
            }

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
                    Y = (short)(oldVert.Y + clickChange),
                    Z = oldVert.Z
                };
                newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex));
            }

            // Refresh
            vertices = room.RoomData.Vertices.ToList();

            // Get the tile face that matches the vertex list
            List<TRFace4> rectangles = room.RoomData.Rectangles.ToList();
            TRFace4 floorFace = rectangles.Find(r => r.Vertices.ToList().All(oldVertIndices.Contains));

            // If the floor has been lowered (remember +Clicks = move down, -Clicks = move up)
            // then the sides will also need lowering.
            if (Clicks > 0)
            {
                // Find faces that share 2 of the old vertices
                int floorY = room.RoomData.Vertices[floorFace.Vertices[0]].Vertex.Y;
                foreach (TRFace4 face in rectangles)
                {
                    if (face == floorFace)
                    {
                        continue;
                    }

                    List<ushort> faceVerts = face.Vertices.ToList();
                    List<ushort> sharedVerts = faceVerts.Where(oldVertIndices.Contains).ToList();
                    List<ushort> uniqueVerts = faceVerts.Except(sharedVerts).ToList();
                    if (sharedVerts.Count == 2 && uniqueVerts.Count == 2)
                    {
                        foreach (ushort sharedVert in sharedVerts)
                        {
                            int i = faceVerts.IndexOf(sharedVert);
                            TRVertex oldVert = vertices[sharedVert].Vertex;
                            foreach (ushort newVert in newVertIndices)
                            {
                                TRVertex newVertex = vertices[newVert].Vertex;
                                if (newVertex.X == oldVert.X && newVertex.Z == oldVert.Z)
                                {
                                    faceVerts[i] = newVert;
                                }
                            }
                        }
                        face.Vertices = faceVerts.ToArray();
                    }
                }
            }

            // Now change the floor face's vertices, and its texture provided we want to.
            if (floorFace != null && !RetainOriginalFloor)
            {
                floorFace.Vertices = newVertIndices.ToArray();
                if (FloorTexture != ushort.MaxValue)
                {
                    floorFace.Texture = FloorTexture;
                }
            }

            // Make faces for the new platform's sides if we have clicked up, again provided we want to.
            if (Clicks < 0 && SideTexture != ushort.MaxValue)
            {
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
            }

            // Save the new faces
            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Now shift the actual sector info
            sector.Floor += Clicks;
            level.Boxes[sector.BoxIndex].TrueFloor = (short)(sector.Floor * ClickSize);

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            foreach (TR2Entity entity in level.Entities)
            {
                if (entity.Room == Location.Room)
                {
                    TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, fdc);
                    if (entitySector == sector)
                    {
                        entity.Y += clickChange;
                    }
                }
            }
        }
    }
}