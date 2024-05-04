using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

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

    public override void ApplyToLevel(TR1Level level)
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

    public override void ApplyToLevel(TR3Level level)
    {
        if (Clicks == 0)
        {
            return;
        }

        MoveFloor(level);
    }

    private void MoveFloor(TR1Level level)
    {
        int clickChange = Clicks * TRConsts.Step1;

        EMLevelData data = GetData(level);

        EMLocation location = data.ConvertLocation(Location);
        TR1Room room = level.Rooms[location.Room];
        TRRoomSector sector = level.GetRoomSector(location);
        int sectorIndex = room.Sectors.IndexOf(sector);

        // Find the current vertices for this tile
        short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
        short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);
        short y = (short)(sector.Floor * TRConsts.Step1);

        List<ushort> oldVertIndices = new();

        List<TRVertex> defVerts = GetTileVertices(x, y, z, false);
        // Check the Y vals are unanimous because we currently only support raising/lowering flat surfaces
        if (!defVerts.All(v => v.Y == defVerts[0].Y))
        {
            throw new NotImplementedException("Floor manipulation is limited to flat surfaces only.");
        }

        for (int i = 0; i < defVerts.Count; i++)
        {
            TRVertex vert = defVerts[i];
            int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
            if (vi != -1)
            {
                oldVertIndices.Add((ushort)vi);
            }
            else
            {
                oldVertIndices.Add((ushort)CreateRoomVertex(room, vert));
            }
        }

        // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
        // those and we need the originals for the new sides to this platform.
        List<ushort> newVertIndices = new();
        foreach (ushort vert in oldVertIndices)
        {
            TR1RoomVertex oldRoomVertex = room.Mesh.Vertices[vert];
            TRVertex oldVert = room.Mesh.Vertices[vert].Vertex;
            TRVertex newVertex = new()
            {
                X = oldVert.X,
                Y = (short)(oldVert.Y + clickChange),
                Z = oldVert.Z
            };
            newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting));
        }

        // Get the tile face that matches the vertex list
        TRFace floorFace = room.Mesh.Rectangles.Find(r => r.Vertices.ToList().All(oldVertIndices.Contains));

        // If the floor has been lowered (remember +Clicks = move down, -Clicks = move up)
        // then the sides will also need lowering.
        if (Clicks > 0)
        {
            // Find faces that share 2 of the old vertices
            int floorY = room.Mesh.Vertices[floorFace.Vertices[0]].Vertex.Y;
            foreach (TRFace face in room.Mesh.Rectangles)
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
                        TRVertex oldVert = room.Mesh.Vertices[sharedVert].Vertex;
                        foreach (ushort newVert in newVertIndices)
                        {
                            TRVertex newVertex = room.Mesh.Vertices[newVert].Vertex;
                            if (newVertex.X == oldVert.X && newVertex.Z == oldVert.Z)
                            {
                                faceVerts[i] = newVert;
                            }
                        }
                    }
                    face.Vertices = faceVerts;
                }
            }
        }

        // Now change the floor face's vertices, and its texture provided we want to.
        if (floorFace != null && !RetainOriginalFloor)
        {
            floorFace.Vertices = newVertIndices;
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
                    room.Mesh.Rectangles.Add(new()
                    {
                        Texture = SideTexture,
                        Vertices = new()
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


        // Now shift the actual sector info and adjust the box if necessary.
        // Make the floor solid too.
        sector.Floor += Clicks;
        sector.RoomBelow = TRConsts.NoRoom;
        AlterSectorBox(level, room, sectorIndex, level.Rooms);

        // Move any entities that share the same floor sector up or down the relevant number of clicks
        foreach (TR1Entity entity in level.Entities)
        {
            if (entity.Room == location.Room)
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.Y += clickChange;
                }
            }
        }
    }

    private void MoveFloor(TR2Level level)
    {
        // Find the vertices of the current floor for the tile at the given location, create 4 additional 
        // vertices on top and make new TRFace entries for the sides so the platform isn't floating.
        // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with.

        int clickChange = Clicks * TRConsts.Step1;

        EMLevelData data = GetData(level);

        EMLocation location = data.ConvertLocation(Location);
        TR2Room room = level.Rooms[location.Room];
        TRRoomSector sector = level.GetRoomSector(location);
        int sectorIndex = room.Sectors.IndexOf(sector);

        // Find the current vertices for this tile
        short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
        short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);
        short y = (short)(sector.Floor * TRConsts.Step1);

        List<ushort> oldVertIndices = new();

        List<TRVertex> defVerts = GetTileVertices(x, y, z, false);
        // Check the Y vals are unanimous because we currently only support raising/lowering flat surfaces
        if (!defVerts.All(v => v.Y == defVerts[0].Y))
        {
            throw new NotImplementedException("Floor manipulation is limited to flat surfaces only.");
        }

        for (int i = 0; i < defVerts.Count; i++)
        {
            TRVertex vert = defVerts[i];
            int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
            if (vi != -1)
            {
                oldVertIndices.Add((ushort)vi);
            }
            else
            {
                oldVertIndices.Add((ushort)CreateRoomVertex(room, vert));
            }
        }

        // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
        // those and we need the originals for the new sides to this platform.
        List<ushort> newVertIndices = new();
        foreach (ushort vert in oldVertIndices)
        {
            TR2RoomVertex oldRoomVertex = room.Mesh.Vertices[vert];
            TRVertex oldVert = room.Mesh.Vertices[vert].Vertex;
            TRVertex newVertex = new()
            {
                X = oldVert.X,
                Y = (short)(oldVert.Y + clickChange),
                Z = oldVert.Z
            };
            newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting, oldRoomVertex.Lighting2));
        }

        // Get the tile face that matches the vertex list
        TRFace floorFace = room.Mesh.Rectangles.Find(r => r.Vertices.ToList().All(oldVertIndices.Contains));

        // If the floor has been lowered (remember +Clicks = move down, -Clicks = move up)
        // then the sides will also need lowering.
        if (Clicks > 0)
        {
            // Find faces that share 2 of the old vertices
            int floorY = room.Mesh.Vertices[floorFace.Vertices[0]].Vertex.Y;
            foreach (TRFace face in room.Mesh.Rectangles)
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
                        TRVertex oldVert = room.Mesh.Vertices[sharedVert].Vertex;
                        foreach (ushort newVert in newVertIndices)
                        {
                            TRVertex newVertex = room.Mesh.Vertices[newVert].Vertex;
                            if (newVertex.X == oldVert.X && newVertex.Z == oldVert.Z)
                            {
                                faceVerts[i] = newVert;
                            }
                        }
                    }
                    face.Vertices = faceVerts;
                }
            }
        }

        // Now change the floor face's vertices, and its texture provided we want to.
        if (floorFace != null && !RetainOriginalFloor)
        {
            floorFace.Vertices = newVertIndices;
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
                    room.Mesh.Rectangles.Add(new()
                    {
                        Texture = SideTexture,
                        Vertices = new()
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

        // Now shift the actual sector info and adjust the box if necessary.
        // Make the floor solid too.
        sector.Floor += Clicks;
        sector.RoomBelow = TRConsts.NoRoom;
        AlterSectorBox(level, room, sectorIndex, level.Rooms);

        // Move any entities that share the same floor sector up or down the relevant number of clicks
        foreach (TR2Entity entity in level.Entities)
        {
            if (entity.Room == location.Room)
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.Y += clickChange;
                }
            }
        }
    }

    private void MoveFloor(TR3Level level)
    {
        // Find the vertices of the current floor for the tile at the given location, create 4 additional 
        // vertices on top and make new TRFace entries for the sides so the platform isn't floating.
        // TODO: how to handle raising/lowering slants, all of this assumes a flat floor to begin with.

        int clickChange = Clicks * TRConsts.Step1;

        EMLevelData data = GetData(level);

        EMLocation location = data.ConvertLocation(Location);
        TR3Room room = level.Rooms[location.Room];
        TRRoomSector sector = level.GetRoomSector(location);
        int sectorIndex = room.Sectors.IndexOf(sector);

        // Find the current vertices for this tile
        short x = (short)(sectorIndex / room.NumZSectors * TRConsts.Step4);
        short z = (short)(sectorIndex % room.NumZSectors * TRConsts.Step4);
        short y = (short)(sector.Floor * TRConsts.Step1);

        List<ushort> oldVertIndices = new();

        List<TRVertex> defVerts = GetTileVertices(x, y, z, false);
        // Check the Y vals are unanimous because we currently only support raising/lowering flat surfaces
        if (!defVerts.All(v => v.Y == defVerts[0].Y))
        {
            throw new NotImplementedException("Floor manipulation is limited to flat surfaces only.");
        }

        for (int i = 0; i < defVerts.Count; i++)
        {
            TRVertex vert = defVerts[i];
            int vi = room.Mesh.Vertices.FindIndex(v => v.Vertex.X == vert.X && v.Vertex.Z == vert.Z && v.Vertex.Y == vert.Y);
            if (vi != -1)
            {
                oldVertIndices.Add((ushort)vi);
            }
        }

        // Create new vertices - we can't just change the original vertex Y vals as adjoining tiles also use 
        // those and we need the originals for the new sides to this platform.
        List<ushort> newVertIndices = new();
        foreach (ushort vert in oldVertIndices)
        {
            TR3RoomVertex oldRoomVertex = room.Mesh.Vertices[vert];
            TRVertex oldVert = room.Mesh.Vertices[vert].Vertex;
            TRVertex newVertex = new()
            {
                X = oldVert.X,
                Y = (short)(oldVert.Y + clickChange),
                Z = oldVert.Z
            };
            newVertIndices.Add((ushort)CreateRoomVertex(room, newVertex, oldRoomVertex.Lighting, oldRoomVertex.Colour, oldRoomVertex.UseCaustics, oldRoomVertex.UseWaveMovement));
        }

        // Get the tile face that matches the vertex list
        TRFace floorFace = room.Mesh.Rectangles.Find(r => r.Vertices.ToList().All(oldVertIndices.Contains));

        // If the floor has been lowered (remember +Clicks = move down, -Clicks = move up)
        // then the sides will also need lowering.
        if (Clicks > 0)
        {
            // Find faces that share 2 of the old vertices
            int floorY = room.Mesh.Vertices[floorFace.Vertices[0]].Vertex.Y;
            foreach (TRFace face in room.Mesh.Rectangles)
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
                        TRVertex oldVert = room.Mesh.Vertices[sharedVert].Vertex;
                        foreach (ushort newVert in newVertIndices)
                        {
                            TRVertex newVertex = room.Mesh.Vertices[newVert].Vertex;
                            if (newVertex.X == oldVert.X && newVertex.Z == oldVert.Z)
                            {
                                faceVerts[i] = newVert;
                            }
                        }
                    }
                    face.Vertices = faceVerts;
                }
            }
        }

        // Now change the floor face's vertices, and its texture provided we want to.
        if (floorFace != null && !RetainOriginalFloor)
        {
            floorFace.Vertices = newVertIndices;
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
                    room.Mesh.Rectangles.Add(new()
                    {
                        Texture = SideTexture,
                        Vertices = new()
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

        // Now shift the actual sector info and adjust the box if necessary
        sector.Floor += Clicks;
        AlterSectorBox(level, room, sectorIndex, level.Rooms);

        // Move any entities that share the same floor sector up or down the relevant number of clicks
        foreach (TR3Entity entity in level.Entities)
        {
            if (entity.Room == location.Room)
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.Y += clickChange;
                }
            }
        }
    }

    private static void AlterSectorBox(TRLevelBase level, TRRoom room, int sectorIndex, IEnumerable<TRRoom> allRooms)
    {
        TRRoomSector sector = room.Sectors[sectorIndex];
        if (sector.BoxIndex == TRConsts.NoBox)
        {
            return;
        }

        if (allRooms.Sum(r => r.Sectors.Count(s => s.BoxIndex == sector.BoxIndex)) == 1)
        {
            // The box used by this sector is unique to this sector, so we can
            // simply change the existing floor height to match the sector.
            level.Boxes[sector.BoxIndex].TrueFloor = (short)(sector.Floor * TRConsts.Step1);
        }
        else
        {
            ushort currentBoxIndex = sector.BoxIndex;
            ushort newBoxIndex = (ushort)level.Boxes.Count;

            // Anything that has the current box as an overlap will need to also have the
            // new box, or if this is the current box, it will need the new box linked to it.
            for (int i = 0; i < level.Boxes.Count; i++)
            {
                TRBox otherBox = level.Boxes[i];
                if (otherBox.Overlaps.Contains(currentBoxIndex) || i == currentBoxIndex)
                {
                    otherBox.Overlaps.Add(newBoxIndex);
                }
            }

            // Make a new box for the sector.
            uint xmin = (uint)(room.Info.X + (sectorIndex / room.NumZSectors) * TRConsts.Step4);
            uint zmin = (uint)(room.Info.Z + (sectorIndex % room.NumZSectors) * TRConsts.Step4);
            TRBox box = new()
            {
                XMin = xmin,
                ZMin = zmin,
                XMax = xmin + TRConsts.Step4,
                ZMax = zmin + TRConsts.Step4,
                TrueFloor = (short)(sector.Floor * TRConsts.Step1),
                Zone = level.Boxes[sector.BoxIndex].Zone.Clone()
            };

            // Point the sector to the new box, and save it to the level
            sector.BoxIndex = newBoxIndex;
            level.Boxes.Add(box);

            // Finally add the previous box as a neighbour to the new one.
            box.Overlaps.Add(currentBoxIndex);
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
}
