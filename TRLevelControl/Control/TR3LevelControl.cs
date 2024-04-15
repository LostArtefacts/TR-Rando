using System.Diagnostics;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR3LevelControl : TRLevelControlBase<TR3Level>
{
    protected override TR3Level CreateLevel(TRFileVersion version)
    {
        TR3Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR3,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR3a, TRFileVersion.TR3b);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        // Colour palettes and textures
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize);
        _level.Palette16 = reader.ReadColour4s(TRConsts.PaletteSize);

        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);
        _level.Images16 = reader.ReadImage16s(numImages);

        // Unused, always 0 in OG
        _level.Version.LevelNumber = reader.ReadUInt32();

        //Rooms
        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR3Room room = new()
            {
                //Grab info
                Info = new TRRoomInfo
                {
                    X = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    YBottom = reader.ReadInt32(),
                    YTop = reader.ReadInt32()
                },

                //Grab data
                NumDataWords = reader.ReadUInt32()
            };
            _level.Rooms.Add(room);

            room.Data = new ushort[room.NumDataWords];
            for (int j = 0; j < room.NumDataWords; j++)
            {
                room.Data[j] = reader.ReadUInt16();
            }

            //Store what we just read
            room.RoomData = ConvertToRoomData(room);

            //Portals
            room.NumPortals = reader.ReadUInt16();
            room.Portals = new TRRoomPortal[room.NumPortals];
            for (int j = 0; j < room.NumPortals; j++)
            {
                room.Portals[j] = TR2FileReadUtilities.ReadRoomPortal(reader);
            }

            //Sectors
            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = new TRRoomSector[room.NumXSectors * room.NumZSectors];
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors[j] = TR2FileReadUtilities.ReadRoomSector(reader);
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            room.NumLights = reader.ReadUInt16();
            room.Lights = new TR3RoomLight[room.NumLights];
            for (int j = 0; j < room.NumLights; j++)
            {
                room.Lights[j] = TR3FileReadUtilities.ReadRoomLight(reader);
            }

            //Static meshes
            room.NumStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new TR3RoomStaticMesh[room.NumStaticMeshes];
            for (int j = 0; j < room.NumStaticMeshes; j++)
            {
                room.StaticMeshes[j] = TR3FileReadUtilities.ReadRoomStaticMesh(reader);
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();

            //New TR3 room info
            room.WaterScheme = reader.ReadByte();
            room.ReverbInfo = reader.ReadByte();
            room.Filler = reader.ReadByte();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();

        //Mesh Data
        //This tells us how much mesh data (# of words/uint16s) coming up
        //just like the rooms previously.
        _level.NumMeshData = reader.ReadUInt32();
        _level.RawMeshData = new ushort[_level.NumMeshData];

        for (int i = 0; i < _level.NumMeshData; i++)
        {
            _level.RawMeshData[i] = reader.ReadUInt16();
        }

        //Mesh Pointers
        _level.NumMeshPointers = reader.ReadUInt32();
        _level.MeshPointers = new uint[_level.NumMeshPointers];

        for (int i = 0; i < _level.NumMeshPointers; i++)
        {
            _level.MeshPointers[i] = reader.ReadUInt32();
        }

        //Mesh Construction
        //level.Meshes = ConstructMeshData(level.NumMeshData, level.NumMeshPointers, level.RawMeshData);
        _level.Meshes = ConstructMeshData(_level.MeshPointers, _level.RawMeshData);

        //Animations
        uint numAnimations = reader.ReadUInt32();
        _level.Animations = new();
        for (int i = 0; i < numAnimations; i++)
        {
            _level.Animations.Add(TR2FileReadUtilities.ReadAnimation(reader));
        }

        //State Changes
        uint numStateChanges = reader.ReadUInt32();
        _level.StateChanges = new();
        for (int i = 0; i < numStateChanges; i++)
        {
            _level.StateChanges.Add(TR2FileReadUtilities.ReadStateChange(reader));
        }

        //Animation Dispatches
        uint numAnimDispatches = reader.ReadUInt32();
        _level.AnimDispatches = new();
        for (int i = 0; i < numAnimDispatches; i++)
        {
            _level.AnimDispatches.Add(TR2FileReadUtilities.ReadAnimDispatch(reader));
        }

        //Animation Commands
        uint numAnimCommands = reader.ReadUInt32();
        _level.AnimCommands = new();
        for (int i = 0; i < numAnimCommands; i++)
        {
            _level.AnimCommands.Add(TR2FileReadUtilities.ReadAnimCommand(reader));
        }

        //Mesh Trees
        uint numMeshTrees = reader.ReadUInt32() / 4;
        _level.MeshTrees = new();
        for (int i = 0; i < numMeshTrees; i++)
        {
            _level.MeshTrees.Add(TR2FileReadUtilities.ReadMeshTreeNode(reader));
        }

        //Frames
        uint numFrames = reader.ReadUInt32();
        _level.Frames = new();
        for (int i = 0; i < numFrames; i++)
        {
            _level.Frames.Add(reader.ReadUInt16());
        }

        //Models
        uint numModels = reader.ReadUInt32();
        _level.Models = new();
        for (int i = 0; i < numModels; i++)
        {
            _level.Models.Add(TR2FileReadUtilities.ReadModel(reader));
        }

        //Static Meshes
        uint numStaticMeshes = reader.ReadUInt32();
        _level.StaticMeshes = new();
        for (int i = 0; i < numStaticMeshes; i++)
        {
            _level.StaticMeshes.Add(TR2FileReadUtilities.ReadStaticMesh(reader));
        }

        //Object Textures - in TR3 this is now after animated textures

        //Sprite Textures
        _level.NumSpriteTextures = reader.ReadUInt32();
        _level.SpriteTextures = new TRSpriteTexture[_level.NumSpriteTextures];

        for (int i = 0; i < _level.NumSpriteTextures; i++)
        {
            _level.SpriteTextures[i] = TR2FileReadUtilities.ReadSpriteTexture(reader);
        }

        //Sprite Sequences
        _level.NumSpriteSequences = reader.ReadUInt32();
        _level.SpriteSequences = new TRSpriteSequence[_level.NumSpriteSequences];

        for (int i = 0; i < _level.NumSpriteSequences; i++)
        {
            _level.SpriteSequences[i] = TR2FileReadUtilities.ReadSpriteSequence(reader);
        }

        uint numCameras = reader.ReadUInt32();
        _level.Cameras = new();
        for (int i = 0; i < numCameras; i++)
        {
            _level.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        uint numSoundSources = reader.ReadUInt32();
        _level.SoundSources = new();
        for (int i = 0; i < numSoundSources; i++)
        {
            _level.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }

        //Boxes
        uint numBoxes = reader.ReadUInt32();
        _level.Boxes = new();
        for (int i = 0; i < numBoxes; i++)
        {
            _level.Boxes.Add(TR2FileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        _level.Overlaps = reader.ReadUInt16s(numOverlaps).ToList();

        ushort[] zoneData = reader.ReadUInt16s(numBoxes * 10);
        _level.Zones = TR2BoxUtilities.ReadZones(numBoxes, zoneData);

        //Animated Textures - the data stores the total number of ushorts to read (NumAnimatedTextures)
        //followed by a ushort to describe the number of actual texture group objects.
        _level.NumAnimatedTextures = reader.ReadUInt32();
        _level.AnimatedTextures = new TRAnimatedTexture[reader.ReadUInt16()];
        for (int i = 0; i < _level.AnimatedTextures.Length; i++)
        {
            _level.AnimatedTextures[i] = TR2FileReadUtilities.ReadAnimatedTexture(reader);
        }

        //Object Textures - in TR3 this is now after animated textures
        _level.NumObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new TRObjectTexture[_level.NumObjectTextures];

        for (int i = 0; i < _level.NumObjectTextures; i++)
        {
            _level.ObjectTextures[i] = TR2FileReadUtilities.ReadObjectTexture(reader);
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR3Entities(numEntities);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));

        //Cinematic Frames
        ushort numCinematicFrames = reader.ReadUInt16();
        _level.CinematicFrames = new();
        for (int i = 0; i < numCinematicFrames; i++)
        {
            _level.CinematicFrames.Add(TR2FileReadUtilities.ReadCinematicFrame(reader));
        }

        //Demo Data
        _level.NumDemoData = reader.ReadUInt16();
        _level.DemoData = new byte[_level.NumDemoData];

        for (int i = 0; i < _level.NumDemoData; i++)
        {
            _level.DemoData[i] = reader.ReadByte();
        }

        //Sound Map (370 shorts = 740 bytes) & Sound Details
        _level.SoundMap = new short[370];

        for (int i = 0; i < _level.SoundMap.Length; i++)
        {
            _level.SoundMap[i] = reader.ReadInt16();
        }

        _level.NumSoundDetails = reader.ReadUInt32();
        _level.SoundDetails = new TR3SoundDetails[_level.NumSoundDetails];

        for (int i = 0; i < _level.NumSoundDetails; i++)
        {
            _level.SoundDetails[i] = TR3FileReadUtilities.ReadSoundDetails(reader);
        }

        //Samples
        _level.NumSampleIndices = reader.ReadUInt32();
        _level.SampleIndices = new uint[_level.NumSampleIndices];

        for (int i = 0; i < _level.NumSampleIndices; i++)
        {
            _level.SampleIndices[i] = reader.ReadUInt32();
        }
    }

    protected override void Write(TRLevelWriter writer)
    {
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        Debug.Assert(_level.Palette16.Count == TRConsts.PaletteSize);
        writer.Write(_level.Palette);
        writer.Write(_level.Palette16);

        Debug.Assert(_level.Images8.Count == _level.Images16.Count);
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);
        writer.Write(_level.Images16);

        writer.Write(_level.Version.LevelNumber);

        writer.Write((ushort)_level.Rooms.Count);
        foreach (TR3Room room in _level.Rooms) { writer.Write(room.Serialize()); }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);

        writer.Write(_level.NumMeshData);
        foreach (TRMesh mesh in _level.Meshes) { writer.Write(mesh.Serialize()); }
        writer.Write(_level.NumMeshPointers);
        foreach (uint ptr in _level.MeshPointers) { writer.Write(ptr); }

        writer.Write((uint)_level.Animations.Count);
        foreach (TRAnimation anim in _level.Animations) { writer.Write(anim.Serialize()); }
        writer.Write((uint)_level.StateChanges.Count);
        foreach (TRStateChange statec in _level.StateChanges) { writer.Write(statec.Serialize()); }
        writer.Write((uint)_level.AnimDispatches.Count);
        foreach (TRAnimDispatch dispatch in _level.AnimDispatches) { writer.Write(dispatch.Serialize()); }
        writer.Write((uint)_level.AnimCommands.Count);
        foreach (TRAnimCommand cmd in _level.AnimCommands) { writer.Write(cmd.Serialize()); }
        writer.Write((uint)_level.MeshTrees.Count * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
        foreach (TRMeshTreeNode node in _level.MeshTrees) { writer.Write(node.Serialize()); }
        writer.Write((uint)_level.Frames.Count);
        foreach (ushort frame in _level.Frames) { writer.Write(frame); }

        writer.Write((uint)_level.Models.Count);
        foreach (TRModel model in _level.Models) { writer.Write(model.Serialize()); }
        writer.Write((uint)_level.StaticMeshes.Count);
        foreach (TRStaticMesh mesh in _level.StaticMeshes) { writer.Write(mesh.Serialize()); }

        writer.Write(_level.NumSpriteTextures);
        foreach (TRSpriteTexture tex in _level.SpriteTextures) { writer.Write(tex.Serialize()); }
        writer.Write(_level.NumSpriteSequences);
        foreach (TRSpriteSequence sequence in _level.SpriteSequences) { writer.Write(sequence.Serialize()); }
        
        writer.Write((uint)_level.Cameras.Count);
        foreach (TRCamera cam in _level.Cameras) { writer.Write(cam.Serialize()); }

        writer.Write((uint)_level.SoundSources.Count);
        foreach (TRSoundSource src in _level.SoundSources) { writer.Write(src.Serialize()); }

        writer.Write((uint)_level.Boxes.Count);
        foreach (TR2Box box in _level.Boxes) { writer.Write(box.Serialize()); }
        writer.Write((uint)_level.Overlaps.Count);
        writer.Write(_level.Overlaps);
        writer.Write(TR2BoxUtilities.FlattenZones(_level.Zones));

        writer.Write(_level.NumAnimatedTextures);
        writer.Write((ushort)_level.AnimatedTextures.Length);
        foreach (TRAnimatedTexture texture in _level.AnimatedTextures) { writer.Write(texture.Serialize()); }
        writer.Write(_level.NumObjectTextures);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }

        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        writer.Write(_level.LightMap.ToArray());

        writer.Write((ushort)_level.CinematicFrames.Count);
        foreach (TRCinematicFrame cineframe in _level.CinematicFrames) { writer.Write(cineframe.Serialize()); }

        writer.Write(_level.NumDemoData);
        writer.Write(_level.DemoData);

        foreach (short sound in _level.SoundMap) { writer.Write(sound); }
        writer.Write(_level.NumSoundDetails);
        foreach (TR3SoundDetails snddetail in _level.SoundDetails) { writer.Write(snddetail.Serialize()); }
        writer.Write(_level.NumSampleIndices);
        foreach (uint index in _level.SampleIndices) { writer.Write(index); }
    }

    private static TR3RoomData ConvertToRoomData(TR3Room room)
    {
        int RoomDataOffset = 0;

        //Grab detailed room data
        TR3RoomData RoomData = new()
        {
            //Room vertices
            NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset])
        };
        RoomData.Vertices = new TR3RoomVertex[RoomData.NumVertices];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumVertices; j++)
        {
            TR3RoomVertex vertex = new()
            {
                Vertex = new TRVertex()
            };

            vertex.Vertex.X = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Vertex.Y = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Vertex.Z = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Lighting = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Attributes = room.Data[RoomDataOffset];
            RoomDataOffset++;
            vertex.Colour = room.Data[RoomDataOffset];
            RoomDataOffset++;

            RoomData.Vertices[j] = vertex;
        }

        //Room rectangles
        RoomData.NumRectangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Rectangles = new TRFace4[RoomData.NumRectangles];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumRectangles; j++)
        {
            TRFace4 face = new()
            {
                Vertices = new ushort[4]
            };
            face.Vertices[0] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[1] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[2] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[3] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Texture = room.Data[RoomDataOffset];
            RoomDataOffset++;

            RoomData.Rectangles[j] = face;
        }

        //Room triangles
        RoomData.NumTriangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Triangles = new TRFace3[RoomData.NumTriangles];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumTriangles; j++)
        {
            TRFace3 face = new()
            {
                Vertices = new ushort[3]
            };
            face.Vertices[0] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[1] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[2] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Texture = room.Data[RoomDataOffset];
            RoomDataOffset++;

            RoomData.Triangles[j] = face;
        }

        //Room sprites
        RoomData.NumSprites = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Sprites = new TRRoomSprite[RoomData.NumSprites];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumSprites; j++)
        {
            TRRoomSprite face = new()
            {
                Vertex = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset])
            };
            RoomDataOffset++;
            face.Texture = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;

            RoomData.Sprites[j] = face;
        }

        Debug.Assert(RoomDataOffset == room.NumDataWords);

        return RoomData;
    }

    private static TRMesh[] ConstructMeshData(uint[] meshPointers, ushort[] rawMeshData)
    {
        byte[] target = new byte[rawMeshData.Length * 2];
        Buffer.BlockCopy(rawMeshData, 0, target, 0, target.Length);

        // The mesh pointer list can contain duplicates so we must make
        // sure to iterate over distinct values only
        meshPointers = meshPointers.Distinct().ToArray();

        List<TRMesh> meshes = new();

        using (MemoryStream ms = new(target))
        using (BinaryReader br = new(ms))
        {
            for (int i = 0; i < meshPointers.Length; i++)
            {
                TRMesh mesh = new();
                meshes.Add(mesh);

                uint meshPointer = meshPointers[i];
                br.BaseStream.Position = meshPointer;

                //Pointer
                mesh.Pointer = meshPointer;

                //Centre
                mesh.Centre = TR2FileReadUtilities.ReadVertex(br);

                //CollRadius
                mesh.CollRadius = br.ReadInt32();

                //Vertices
                mesh.NumVertices = br.ReadInt16();
                mesh.Vertices = new TRVertex[mesh.NumVertices];
                for (int j = 0; j < mesh.NumVertices; j++)
                {
                    mesh.Vertices[j] = TR2FileReadUtilities.ReadVertex(br);
                }

                //Lights or Normals
                mesh.NumNormals = br.ReadInt16();
                if (mesh.NumNormals > 0)
                {
                    mesh.Normals = new TRVertex[mesh.NumNormals];
                    for (int j = 0; j < mesh.NumNormals; j++)
                    {
                        mesh.Normals[j] = TR2FileReadUtilities.ReadVertex(br);
                    }
                }
                else
                {
                    mesh.Lights = new short[Math.Abs(mesh.NumNormals)];
                    for (int j = 0; j < mesh.Lights.Length; j++)
                    {
                        mesh.Lights[j] = br.ReadInt16();
                    }
                }

                //Textured Rectangles
                mesh.NumTexturedRectangles = br.ReadInt16();
                mesh.TexturedRectangles = new TRFace4[mesh.NumTexturedRectangles];
                for (int j = 0; j < mesh.NumTexturedRectangles; j++)
                {
                    mesh.TexturedRectangles[j] = TR2FileReadUtilities.ReadTRFace4(br);
                }

                //Textured Triangles
                mesh.NumTexturedTriangles = br.ReadInt16();
                mesh.TexturedTriangles = new TRFace3[mesh.NumTexturedTriangles];
                for (int j = 0; j < mesh.NumTexturedTriangles; j++)
                {
                    mesh.TexturedTriangles[j] = TR2FileReadUtilities.ReadTRFace3(br);
                }

                //Coloured Rectangles
                mesh.NumColouredRectangles = br.ReadInt16();
                mesh.ColouredRectangles = new TRFace4[mesh.NumColouredRectangles];
                for (int j = 0; j < mesh.NumColouredRectangles; j++)
                {
                    mesh.ColouredRectangles[j] = TR2FileReadUtilities.ReadTRFace4(br);
                }

                //Coloured Triangles
                mesh.NumColouredTriangles = br.ReadInt16();
                mesh.ColouredTriangles = new TRFace3[mesh.NumColouredTriangles];
                for (int j = 0; j < mesh.NumColouredTriangles; j++)
                {
                    mesh.ColouredTriangles[j] = TR2FileReadUtilities.ReadTRFace3(br);
                }

                // There may be alignment padding at the end of the mesh, but rather than
                // storing it, when the mesh is serialized the alignment should be considered.
                // It seems to be 4-byte alignment for mesh data. The basestream position is
                // moved to the next pointer in the next iteration, so we don't need to process
                // the additional data here.
                // See https://www.tombraiderforums.com/archive/index.php/t-215247.html
            }
        }

        return meshes.ToArray();
    }
}
