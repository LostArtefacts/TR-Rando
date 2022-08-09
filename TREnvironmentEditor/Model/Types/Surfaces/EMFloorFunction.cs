using System;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMFloorFunction : BaseEMFunction, ITextureModifier
    {
        public EMLocation Location { get; set; }
        public sbyte Clicks { get; set; }
        public ushort FloorTexture { get; set; }
        public ushort SideTexture { get; set; }
        public bool RetainOriginalFloor { get; set; }
        public int Flags { get; set; } // Which sides to texture - prevents z-fighting

        public EMFloorFunction()
        {
            FloorTexture = ushort.MaxValue;
            SideTexture = ushort.MaxValue;
            Flags = 0xF;
        }

        public override void ApplyToLevel(TRLevel level)
        {
            if (Clicks == 0)
            {
                return;
            }

            MoveFloor(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            if (Clicks == 0)
            {
                return;
            }
            
            MoveFloor(level);
        }

        private void MoveFloor(TRLevel level)
        {
            int clickChange = Clicks * ClickSize;

            EMLevelData data = GetData(level);

            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            short roomNumber = data.ConvertRoom(Location.Room);
            TRRoom room = level.Rooms[roomNumber];
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomNumber, level, fdc);
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);

            // Find the current vertices for this tile
            short x = (short)(sectorIndex / room.NumZSectors * SectorSize);
            short z = (short)(sectorIndex % room.NumZSectors * SectorSize);
            short y = (short)(sector.Floor * ClickSize);

            List<TRRoomVertex> vertices = room.RoomData.Vertices.ToList();
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
                else
                {
                    oldVertIndices.Add((ushort)CreateRoomVertex(room, vert));
                    vertices = room.RoomData.Vertices.ToList();
                }
            }

            // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
            // those and we need the originals for the new sides to this platform.
            List<ushort> newVertIndices = new List<ushort>();
            foreach (ushort vert in oldVertIndices)
            {
                TRRoomVertex oldRoomVertex = vertices[vert];
                TRVertex oldVert = vertices[vert].Vertex;
                TRVertex newVertex = new TRVertex
                {
                    X = oldVert.X,
                    Y = (short)(oldVert.Y + clickChange),
                    Z = oldVert.Z
                };
                newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting));
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
                    // Only texture this side if it's set in the flags
                    if (((1 << i) & Flags) > 0)
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
            }

            // Save the new faces
            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);

            // Now shift the actual sector info and adjust the box if necessary.
            // Make the floor solid too.
            sector.Floor += Clicks;
            sector.RoomBelow = 255;
            AlterSectorBox(level, room, sectorIndex);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            foreach (TREntity entity in level.Entities)
            {
                if (entity.Room == roomNumber)
                {
                    TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, fdc);
                    if (entitySector == sector)
                    {
                        entity.Y += clickChange;
                    }
                }
            }
        }

        private void AlterSectorBox(TRLevel level, TRRoom room, int sectorIndex)
        {
            TRRoomSector sector = room.Sectors[sectorIndex];
            if (sector.BoxIndex == ushort.MaxValue)
            {
                return;
            }

            if (TR1BoxUtilities.GetSectorCount(level, sector.BoxIndex) == 1)
            {
                // The box used by this sector is unique to this sector, so we can
                // simply change the existing floor height to match the sector.
                level.Boxes[sector.BoxIndex].TrueFloor = (short)(sector.Floor * ClickSize);
            }
            else
            {
                ushort currentBoxIndex = sector.BoxIndex;
                ushort newBoxIndex = (ushort)level.NumBoxes;

                // Make a new zone to match the addition of a new box.
                TR1BoxUtilities.DuplicateZone(level, sector.BoxIndex);

                // Add what will be the new box index as an overlap to adjoining boxes.
                GenerateOverlaps(level, sector.BoxIndex, newBoxIndex);

                // Make a new box for the sector.
                uint xmin = (uint)(room.Info.X + (sectorIndex / room.NumZSectors) * SectorSize);
                uint zmin = (uint)(room.Info.Z + (sectorIndex % room.NumZSectors) * SectorSize);
                uint xmax = (uint)(xmin + SectorSize);
                uint zmax = (uint)(zmin + SectorSize);
                TRBox box = new TRBox
                {
                    XMin = xmin,
                    ZMin = zmin,
                    XMax = xmax,
                    ZMax = zmax,
                    TrueFloor = (short)(sector.Floor * ClickSize)
                };

                // Point the sector to the new box, and save it to the level
                sector.BoxIndex = newBoxIndex;
                List<TRBox> boxes = level.Boxes.ToList();
                boxes.Add(box);
                level.Boxes = boxes.ToArray();
                level.NumBoxes++;

                // Finally add the previous box as a neighbour to the new one.
                TR1BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { currentBoxIndex });
            }
        }

        private void GenerateOverlaps(TRLevel level, ushort currentBoxIndex, ushort newBoxIndex)
        {
            for (int i = 0; i < level.NumBoxes; i++)
            {
                TRBox box = level.Boxes[i];
                // Anything that has the current box as an overlap will need
                // to also have the new box, or if this is the current box, it
                // will need the new box linked to it.
                List<ushort> overlaps = TR1BoxUtilities.GetOverlaps(level, box);
                if (overlaps.Contains(currentBoxIndex) || i == currentBoxIndex)
                {
                    // Add the new index and write it back
                    overlaps.Add(newBoxIndex);
                    TR1BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }

        private void MoveFloor(TR2Level level)
        {
            // Find the vertices of the current floor for the tile at the given location, create 4 additional 
            // vertices on top and make new TRFace4 entries for the sides so the platform isn't floating.
            // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with.

            int clickChange = Clicks * ClickSize;

            EMLevelData data = GetData(level);

            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            short roomNumber = data.ConvertRoom(Location.Room);
            TR2Room room = level.Rooms[roomNumber];
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomNumber, level, fdc);
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
                else
                {
                    oldVertIndices.Add((ushort)CreateRoomVertex(room, vert));
                    vertices = room.RoomData.Vertices.ToList();
                }
            }

            // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
            // those and we need the originals for the new sides to this platform.
            List<ushort> newVertIndices = new List<ushort>();
            foreach (ushort vert in oldVertIndices)
            {
                TR2RoomVertex oldRoomVertex = vertices[vert];
                TRVertex oldVert = vertices[vert].Vertex;
                TRVertex newVertex = new TRVertex
                {
                    X = oldVert.X,
                    Y = (short)(oldVert.Y + clickChange),
                    Z = oldVert.Z
                };
                newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting, oldRoomVertex.Lighting2));
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
                    // Only texture this side if it's set in the flags
                    if (((1 << i) & Flags) > 0)
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
            }

            // Save the new faces
            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);

            // Now shift the actual sector info and adjust the box if necessary.
            // Make the floor solid too.
            sector.Floor += Clicks;
            sector.RoomBelow = 255;
            AlterSectorBox(level, room, sectorIndex);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            foreach (TR2Entity entity in level.Entities)
            {
                if (entity.Room == roomNumber)
                {
                    TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, fdc);
                    if (entitySector == sector)
                    {
                        entity.Y += clickChange;
                    }
                }
            }
        }

        private void AlterSectorBox(TR2Level level, TR2Room room, int sectorIndex)
        {
            TRRoomSector sector = room.SectorList[sectorIndex];
            if (sector.BoxIndex == ushort.MaxValue)
            {
                return;
            }

            if (TR2BoxUtilities.GetSectorCount(level, sector.BoxIndex) == 1)
            {
                // The box used by this sector is unique to this sector, so we can
                // simply change the existing floor height to match the sector.
                level.Boxes[sector.BoxIndex].TrueFloor = (short)(sector.Floor * ClickSize);
            }
            else
            {
                ushort currentBoxIndex = sector.BoxIndex;
                ushort newBoxIndex = (ushort)level.NumBoxes;

                // Make a new zone to match the addition of a new box.
                TR2BoxUtilities.DuplicateZone(level, sector.BoxIndex);

                // Add what will be the new box index as an overlap to adjoining boxes.
                GenerateOverlaps(level, sector.BoxIndex, newBoxIndex);

                // Make a new box for the sector.
                byte xmin = (byte)((room.Info.X / SectorSize) + (sectorIndex / room.NumZSectors));
                byte zmin = (byte)((room.Info.Z / SectorSize) + (sectorIndex % room.NumZSectors));
                TR2Box box = new TR2Box
                {
                    XMin = xmin,
                    ZMin = zmin,
                    XMax = (byte)(xmin + 1), // Only 1 tile
                    ZMax = (byte)(zmin + 1),
                    TrueFloor = (short)(sector.Floor * ClickSize)
                };

                // Point the sector to the new box, and save it to the level
                sector.BoxIndex = newBoxIndex;
                List<TR2Box> boxes = level.Boxes.ToList();
                boxes.Add(box);
                level.Boxes = boxes.ToArray();
                level.NumBoxes++;

                // Finally add the previous box as a neighbour to the new one.
                TR2BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { currentBoxIndex });
            }
        }

        private void GenerateOverlaps(TR2Level level, ushort currentBoxIndex, ushort newBoxIndex)
        {
            for (int i = 0; i < level.NumBoxes; i++)
            {
                TR2Box box = level.Boxes[i];
                // Anything that has the current box as an overlap will need
                // to also have the new box, or if this is the current box, it
                // will need the new box linked to it.
                List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                if (overlaps.Contains(currentBoxIndex) || i == currentBoxIndex)
                {
                    // Add the new index and write it back
                    overlaps.Add(newBoxIndex);
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            if (indexMap.ContainsKey(FloorTexture))
            {
                FloorTexture = indexMap[FloorTexture];
            }
            if (indexMap.ContainsKey(SideTexture))
            {
                SideTexture = indexMap[SideTexture];
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            if (Clicks == 0)
            {
                return;
            }

            MoveFloor(level);
        }

        private void MoveFloor(TR3Level level)
        {
            // Find the vertices of the current floor for the tile at the given location, create 4 additional 
            // vertices on top and make new TRFace4 entries for the sides so the platform isn't floating.
            // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with.

            int clickChange = Clicks * ClickSize;

            EMLevelData data = GetData(level);

            FDControl fdc = new FDControl();
            fdc.ParseFromLevel(level);

            short roomNumber = data.ConvertRoom(Location.Room);
            TR3Room room = level.Rooms[roomNumber];
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomNumber, level, fdc);
            int sectorIndex = room.Sectors.ToList().IndexOf(sector);

            // Find the current vertices for this tile
            short x = (short)(sectorIndex / room.NumZSectors * SectorSize);
            short z = (short)(sectorIndex % room.NumZSectors * SectorSize);
            short y = (short)(sector.Floor * ClickSize);

            List<TR3RoomVertex> vertices = room.RoomData.Vertices.ToList();
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
                TR3RoomVertex oldRoomVertex = vertices[vert];
                TRVertex oldVert = vertices[vert].Vertex;
                TRVertex newVertex = new TRVertex
                {
                    X = oldVert.X,
                    Y = (short)(oldVert.Y + clickChange),
                    Z = oldVert.Z
                };
                newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting, oldRoomVertex.Colour, oldRoomVertex.UseCaustics, oldRoomVertex.UseWaveMovement));
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
                    // Only texture this side if it's set in the flags
                    if (((1 << i) & Flags) > 0)
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
            }

            // Save the new faces
            room.RoomData.Rectangles = rectangles.ToArray();
            room.RoomData.NumRectangles = (short)rectangles.Count;

            // Account for the added faces
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);

            // Now shift the actual sector info and adjust the box if necessary
            sector.Floor += Clicks;
            AlterSectorBox(level, room, sectorIndex);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            foreach (TR2Entity entity in level.Entities)
            {
                if (entity.Room == roomNumber)
                {
                    TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, fdc);
                    if (entitySector == sector)
                    {
                        entity.Y += clickChange;
                    }
                }
            }
        }

        private void AlterSectorBox(TR3Level level, TR3Room room, int sectorIndex)
        {
            TRRoomSector sector = room.Sectors[sectorIndex];
            if (sector.BoxIndex == ushort.MaxValue)
            {
                return;
            }

            ushort currentBoxIndex = (ushort)((sector.BoxIndex & 0x7FF0) >> 4);
            int currentMaterial = sector.BoxIndex & 0x000F;

            if (TR2BoxUtilities.GetSectorCount(level, sector.BoxIndex) == 1)
            {
                // The box used by this sector is unique to this sector, so we can
                // simply change the existing floor height to match the sector.
                level.Boxes[currentBoxIndex].TrueFloor = (short)(sector.Floor * ClickSize);
            }
            else
            {
                ushort newBoxIndex = (ushort)level.NumBoxes;

                // Make a new zone to match the addition of a new box.
                TR2BoxUtilities.DuplicateZone(level, currentBoxIndex);

                // Add what will be the new box index as an overlap to adjoining boxes.
                GenerateOverlaps(level, currentBoxIndex, newBoxIndex);

                // Make a new box for the sector.
                byte xmin = (byte)((room.Info.X / SectorSize) + (sectorIndex / room.NumZSectors));
                byte zmin = (byte)((room.Info.Z / SectorSize) + (sectorIndex % room.NumZSectors));
                TR2Box box = new TR2Box
                {
                    XMin = xmin,
                    ZMin = zmin,
                    XMax = (byte)(xmin + 1), // Only 1 tile
                    ZMax = (byte)(zmin + 1),
                    TrueFloor = (short)(sector.Floor * ClickSize)
                };

                // Point the sector to the new box, and save it to the level
                newBoxIndex <<= 4;
                newBoxIndex |= (ushort)currentMaterial;
                sector.BoxIndex = newBoxIndex;
                List<TR2Box> boxes = level.Boxes.ToList();
                boxes.Add(box);
                level.Boxes = boxes.ToArray();
                level.NumBoxes++;

                // Finally add the previous box as a neighbour to the new one.
                TR2BoxUtilities.UpdateOverlaps(level, box, new List<ushort> { currentBoxIndex });
            }
        }

        private void GenerateOverlaps(TR3Level level, ushort currentBoxIndex, ushort newBoxIndex)
        {
            for (int i = 0; i < level.NumBoxes; i++)
            {
                TR2Box box = level.Boxes[i];
                // Anything that has the current box as an overlap will need
                // to also have the new box, or if this is the current box, it
                // will need the new box linked to it.
                List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(level, box);
                if (overlaps.Contains(currentBoxIndex) || i == currentBoxIndex)
                {
                    // Add the new index and write it back
                    overlaps.Add(newBoxIndex);
                    TR2BoxUtilities.UpdateOverlaps(level, box, overlaps);
                }
            }
        }
    }
}