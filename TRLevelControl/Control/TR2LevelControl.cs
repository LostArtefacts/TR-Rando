﻿using System.Diagnostics;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR2LevelControl : TRLevelControlBase<TR2Level>
{
    protected override TR2Level CreateLevel(TRFileVersion version)
    {
        TR2Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR2,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR2);
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

        //Rooms
        _level.Unused = reader.ReadUInt32();
        _level.NumRooms = reader.ReadUInt16();
        _level.Rooms = new TR2Room[_level.NumRooms];

        for (int i = 0; i < _level.NumRooms; i++)
        {
            TR2Room room = new()
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
            room.SectorList = new TRRoomSector[room.NumXSectors * room.NumZSectors];
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.SectorList[j] = TR2FileReadUtilities.ReadRoomSector(reader);
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.AmbientIntensity2 = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            room.NumLights = reader.ReadUInt16();
            room.Lights = new TR2RoomLight[room.NumLights];
            for (int j = 0; j < room.NumLights; j++)
            {
                room.Lights[j] = TR2FileReadUtilities.ReadRoomLight(reader);
            }

            //Static meshes
            room.NumStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new TR2RoomStaticMesh[room.NumStaticMeshes];
            for (int j = 0; j < room.NumStaticMeshes; j++)
            {
                room.StaticMeshes[j] = TR2FileReadUtilities.ReadRoomStaticMesh(reader);
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();

            _level.Rooms[i] = room;
        }

        //Floordata
        _level.NumFloorData = reader.ReadUInt32();
        _level.FloorData = new ushort[_level.NumFloorData];

        for (int i = 0; i < _level.NumFloorData; i++)
        {
            _level.FloorData[i] = reader.ReadUInt16();
        }

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
        _level.NumAnimations = reader.ReadUInt32();
        _level.Animations = new TRAnimation[_level.NumAnimations];
        for (int i = 0; i < _level.NumAnimations; i++)
        {
            _level.Animations[i] = TR2FileReadUtilities.ReadAnimation(reader);
        }

        //State Changes
        _level.NumStateChanges = reader.ReadUInt32();
        _level.StateChanges = new TRStateChange[_level.NumStateChanges];
        for (int i = 0; i < _level.NumStateChanges; i++)
        {
            _level.StateChanges[i] = TR2FileReadUtilities.ReadStateChange(reader);
        }

        //Animation Dispatches
        _level.NumAnimDispatches = reader.ReadUInt32();
        _level.AnimDispatches = new TRAnimDispatch[_level.NumAnimDispatches];
        for (int i = 0; i < _level.NumAnimDispatches; i++)
        {
            _level.AnimDispatches[i] = TR2FileReadUtilities.ReadAnimDispatch(reader);
        }

        //Animation Commands
        _level.NumAnimCommands = reader.ReadUInt32();
        _level.AnimCommands = new TRAnimCommand[_level.NumAnimCommands];
        for (int i = 0; i < _level.NumAnimCommands; i++)
        {
            _level.AnimCommands[i] = TR2FileReadUtilities.ReadAnimCommand(reader);
        }

        //Mesh Trees
        _level.NumMeshTrees = reader.ReadUInt32();
        _level.NumMeshTrees /= 4;
        _level.MeshTrees = new TRMeshTreeNode[_level.NumMeshTrees];
        for (int i = 0; i < _level.NumMeshTrees; i++)
        {
            _level.MeshTrees[i] = TR2FileReadUtilities.ReadMeshTreeNode(reader);
        }

        //Frames
        _level.NumFrames = reader.ReadUInt32();
        _level.Frames = new ushort[_level.NumFrames];
        for (int i = 0; i < _level.NumFrames; i++)
        {
            _level.Frames[i] = reader.ReadUInt16();
        }

        //Models
        _level.NumModels = reader.ReadUInt32();
        _level.Models = new TRModel[_level.NumModels];

        for (int i = 0; i < _level.NumModels; i++)
        {
            _level.Models[i] = TR2FileReadUtilities.ReadModel(reader);
        }

        //Static Meshes
        _level.NumStaticMeshes = reader.ReadUInt32();
        _level.StaticMeshes = new TRStaticMesh[_level.NumStaticMeshes];

        for (int i = 0; i < _level.NumStaticMeshes; i++)
        {
            _level.StaticMeshes[i] = TR2FileReadUtilities.ReadStaticMesh(reader);
        }

        //Object Textures
        _level.NumObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new TRObjectTexture[_level.NumObjectTextures];

        for (int i = 0; i < _level.NumObjectTextures; i++)
        {
            _level.ObjectTextures[i] = TR2FileReadUtilities.ReadObjectTexture(reader);
        }

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

        //Cameras
        _level.NumCameras = reader.ReadUInt32();
        _level.Cameras = new TRCamera[_level.NumCameras];

        for (int i = 0; i < _level.NumCameras; i++)
        {
            _level.Cameras[i] = TR2FileReadUtilities.ReadCamera(reader);
        }

        //Sound Sources
        _level.NumSoundSources = reader.ReadUInt32();
        _level.SoundSources = new TRSoundSource[_level.NumSoundSources];

        for (int i = 0; i < _level.NumSoundSources; i++)
        {
            _level.SoundSources[i] = TR2FileReadUtilities.ReadSoundSource(reader);
        }

        //Boxes
        _level.NumBoxes = reader.ReadUInt32();
        _level.Boxes = new TR2Box[_level.NumBoxes];

        for (int i = 0; i < _level.NumBoxes; i++)
        {
            _level.Boxes[i] = TR2FileReadUtilities.ReadBox(reader);
        }

        //Overlaps & Zones
        _level.NumOverlaps = reader.ReadUInt32();
        _level.Overlaps = new ushort[_level.NumOverlaps];

        for (int i = 0; i < _level.NumOverlaps; i++)
        {
            _level.Overlaps[i] = reader.ReadUInt16();
        }

        // Although TRosettaStone references a struct for zones, the data isn't
        // sequential. Instead it's organised by flipmap status, all the groundzone
        // values are then together and the flyzones at the end. TR2BoxUtilities
        // handles the complexity here, so we just pass the raw ushort values.
        ushort[] zoneData = new ushort[_level.NumBoxes * 10];
        for (int i = 0; i < zoneData.Length; i++)
        {
            zoneData[i] = reader.ReadUInt16();
        }
        _level.Zones = TR2BoxUtilities.ReadZones(_level.NumBoxes, zoneData);

        //Animated Textures - the data stores the total number of ushorts to read (NumAnimatedTextures)
        //followed by a ushort to describe the number of actual texture group objects.
        _level.NumAnimatedTextures = reader.ReadUInt32();
        _level.AnimatedTextures = new TRAnimatedTexture[reader.ReadUInt16()];
        for (int i = 0; i < _level.AnimatedTextures.Length; i++)
        {
            _level.AnimatedTextures[i] = TR2FileReadUtilities.ReadAnimatedTexture(reader);
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR2Entities(numEntities);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));

        //Cinematic Frames
        _level.NumCinematicFrames = reader.ReadUInt16();
        _level.CinematicFrames = new TRCinematicFrame[_level.NumCinematicFrames];

        for (int i = 0; i < _level.NumCinematicFrames; i++)
        {
            _level.CinematicFrames[i] = TR2FileReadUtilities.ReadCinematicFrame(reader);
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
        _level.SoundDetails = new TRSoundDetails[_level.NumSoundDetails];

        for (int i = 0; i < _level.NumSoundDetails; i++)
        {
            _level.SoundDetails[i] = TR2FileReadUtilities.ReadSoundDetails(reader);
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

        writer.Write(_level.Unused);

        writer.Write(_level.NumRooms);
        foreach (TR2Room room in _level.Rooms) { writer.Write(room.Serialize()); }
        writer.Write(_level.NumFloorData);
        foreach (ushort data in _level.FloorData) { writer.Write(data); }

        writer.Write(_level.NumMeshData);
        foreach (TRMesh mesh in _level.Meshes) { writer.Write(mesh.Serialize()); }
        writer.Write(_level.NumMeshPointers);
        foreach (uint ptr in _level.MeshPointers) { writer.Write(ptr); }

        writer.Write(_level.NumAnimations);
        foreach (TRAnimation anim in _level.Animations) { writer.Write(anim.Serialize()); }
        writer.Write(_level.NumStateChanges);
        foreach (TRStateChange statec in _level.StateChanges) { writer.Write(statec.Serialize()); }
        writer.Write(_level.NumAnimDispatches);
        foreach (TRAnimDispatch dispatch in _level.AnimDispatches) { writer.Write(dispatch.Serialize()); }
        writer.Write(_level.NumAnimCommands);
        foreach (TRAnimCommand cmd in _level.AnimCommands) { writer.Write(cmd.Serialize()); }
        writer.Write(_level.NumMeshTrees * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
        foreach (TRMeshTreeNode node in _level.MeshTrees) { writer.Write(node.Serialize()); }
        writer.Write(_level.NumFrames);
        foreach (ushort frame in _level.Frames) { writer.Write(frame); }

        writer.Write(_level.NumModels);
        foreach (TRModel model in _level.Models) { writer.Write(model.Serialize()); }
        writer.Write(_level.NumStaticMeshes);
        foreach (TRStaticMesh mesh in _level.StaticMeshes) { writer.Write(mesh.Serialize()); }

        writer.Write(_level.NumObjectTextures);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }
        writer.Write(_level.NumSpriteTextures);
        foreach (TRSpriteTexture tex in _level.SpriteTextures) { writer.Write(tex.Serialize()); }
        writer.Write(_level.NumSpriteSequences);
        foreach (TRSpriteSequence sequence in _level.SpriteSequences) { writer.Write(sequence.Serialize()); }

        writer.Write(_level.NumCameras);
        foreach (TRCamera cam in _level.Cameras) { writer.Write(cam.Serialize()); }

        writer.Write(_level.NumSoundSources);
        foreach (TRSoundSource src in _level.SoundSources) { writer.Write(src.Serialize()); }

        writer.Write(_level.NumBoxes);
        foreach (TR2Box box in _level.Boxes) { writer.Write(box.Serialize()); }
        writer.Write(_level.NumOverlaps);
        foreach (ushort overlap in _level.Overlaps) { writer.Write(overlap); }
        foreach (ushort zone in TR2BoxUtilities.FlattenZones(_level.Zones)) { writer.Write(zone); }

        writer.Write(_level.NumAnimatedTextures);
        writer.Write((ushort)_level.AnimatedTextures.Length);
        foreach (TRAnimatedTexture texture in _level.AnimatedTextures) { writer.Write(texture.Serialize()); }
        
        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        writer.Write(_level.LightMap.ToArray());

        writer.Write(_level.NumCinematicFrames);
        foreach (TRCinematicFrame cineframe in _level.CinematicFrames) { writer.Write(cineframe.Serialize()); }

        writer.Write(_level.NumDemoData);
        writer.Write(_level.DemoData);

        foreach (short sound in _level.SoundMap) { writer.Write(sound); }
        writer.Write(_level.NumSoundDetails);
        foreach (TRSoundDetails snddetail in _level.SoundDetails) { writer.Write(snddetail.Serialize()); }
        writer.Write(_level.NumSampleIndices);
        foreach (uint index in _level.SampleIndices) { writer.Write(index); }
    }

    private static TR2RoomData ConvertToRoomData(TR2Room room)
    {
        int RoomDataOffset = 0;

        //Grab detailed room data
        TR2RoomData RoomData = new()
        {
            //Room vertices
            NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset])
        };
        RoomData.Vertices = new TR2RoomVertex[RoomData.NumVertices];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumVertices; j++)
        {
            TR2RoomVertex vertex = new()
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
            vertex.Lighting2 = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
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
