using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR4FileReadUtilities
{
    public static void PopulateRooms(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.Unused = reader.ReadUInt32();
        lvl.LevelDataChunk.NumRooms = reader.ReadUInt16();
        lvl.LevelDataChunk.Rooms = new TR4Room[lvl.LevelDataChunk.NumRooms];

        for (int i = 0; i < lvl.LevelDataChunk.NumRooms; i++)
        {
            TR4Room room = new()
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
            room.Sectors = new TRRoomSector[room.NumXSectors * room.NumZSectors];
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors[j] = TR2FileReadUtilities.ReadRoomSector(reader);
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            room.NumLights = reader.ReadUInt16();
            room.Lights = new TR4RoomLight[room.NumLights];
            for (int j = 0; j < room.NumLights; j++)
            {
                room.Lights[j] = TR4FileReadUtilities.ReadRoomLight(reader);
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
            room.WaterScheme = reader.ReadByte();
            room.ReverbInfo = reader.ReadByte();
            room.Filler = reader.ReadByte();

            lvl.LevelDataChunk.Rooms[i] = room;
        }
    }

    public static void PopulateFloordata(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.NumFloorData = reader.ReadUInt32();
        lvl.LevelDataChunk.Floordata = new ushort[lvl.LevelDataChunk.NumFloorData];

        for (int i = 0; i < lvl.LevelDataChunk.NumFloorData; i++)
        {
            lvl.LevelDataChunk.Floordata[i] = reader.ReadUInt16();
        }
    }

    public static void PopulateMeshes(BinaryReader reader, TR4Level lvl)
    {
        //Mesh Data
        //This tells us how much mesh data (# of words/uint16s) coming up
        //just like the rooms previously.
        lvl.LevelDataChunk.NumMeshData = reader.ReadUInt32();
        lvl.LevelDataChunk.RawMeshData = new ushort[lvl.LevelDataChunk.NumMeshData];

        for (int i = 0; i < lvl.LevelDataChunk.NumMeshData; i++)
        {
            lvl.LevelDataChunk.RawMeshData[i] = reader.ReadUInt16();
        }

        //Mesh Pointers
        lvl.LevelDataChunk.NumMeshPointers = reader.ReadUInt32();
        lvl.LevelDataChunk.MeshPointers = new uint[lvl.LevelDataChunk.NumMeshPointers];

        for (int i = 0; i < lvl.LevelDataChunk.NumMeshPointers; i++)
        {
            lvl.LevelDataChunk.MeshPointers[i] = reader.ReadUInt32();
        }

        //Mesh Construction
        //level.Meshes = ConstructMeshData(level.NumMeshData, level.NumMeshPointers, level.RawMeshData);
        lvl.LevelDataChunk.Meshes = ConstructMeshData(lvl.LevelDataChunk.MeshPointers, lvl.LevelDataChunk.RawMeshData);
    }

    public static void PopulateAnimations(BinaryReader reader, TR4Level lvl)
    {
        //Animations
        lvl.LevelDataChunk.NumAnimations = reader.ReadUInt32();
        lvl.LevelDataChunk.Animations = new TR4Animation[lvl.LevelDataChunk.NumAnimations];
        for (int i = 0; i < lvl.LevelDataChunk.NumAnimations; i++)
        {
            lvl.LevelDataChunk.Animations[i] = TR4FileReadUtilities.ReadAnimation(reader);
        }

        //State Changes
        lvl.LevelDataChunk.NumStateChanges = reader.ReadUInt32();
        lvl.LevelDataChunk.StateChanges = new TRStateChange[lvl.LevelDataChunk.NumStateChanges];
        for (int i = 0; i < lvl.LevelDataChunk.NumStateChanges; i++)
        {
            lvl.LevelDataChunk.StateChanges[i] = TR2FileReadUtilities.ReadStateChange(reader);
        }

        //Animation Dispatches
        lvl.LevelDataChunk.NumAnimDispatches = reader.ReadUInt32();
        lvl.LevelDataChunk.AnimDispatches = new TRAnimDispatch[lvl.LevelDataChunk.NumAnimDispatches];
        for (int i = 0; i < lvl.LevelDataChunk.NumAnimDispatches; i++)
        {
            lvl.LevelDataChunk.AnimDispatches[i] = TR2FileReadUtilities.ReadAnimDispatch(reader);
        }

        //Animation Commands
        lvl.LevelDataChunk.NumAnimCommands = reader.ReadUInt32();
        lvl.LevelDataChunk.AnimCommands = new TRAnimCommand[lvl.LevelDataChunk.NumAnimCommands];
        for (int i = 0; i < lvl.LevelDataChunk.NumAnimCommands; i++)
        {
            lvl.LevelDataChunk.AnimCommands[i] = TR2FileReadUtilities.ReadAnimCommand(reader);
        }
    }

    public static void PopulateMeshTreesFramesModels(BinaryReader reader, TR4Level lvl)
    {
        //Mesh Trees
        lvl.LevelDataChunk.NumMeshTrees = reader.ReadUInt32();
        lvl.LevelDataChunk.NumMeshTrees /= 4;
        lvl.LevelDataChunk.MeshTrees = new TRMeshTreeNode[lvl.LevelDataChunk.NumMeshTrees];
        for (int i = 0; i < lvl.LevelDataChunk.NumMeshTrees; i++)
        {
            lvl.LevelDataChunk.MeshTrees[i] = TR2FileReadUtilities.ReadMeshTreeNode(reader);
        }

        //Frames
        lvl.LevelDataChunk.NumFrames = reader.ReadUInt32();
        lvl.LevelDataChunk.Frames = new ushort[lvl.LevelDataChunk.NumFrames];
        for (int i = 0; i < lvl.LevelDataChunk.NumFrames; i++)
        {
            lvl.LevelDataChunk.Frames[i] = reader.ReadUInt16();
        }

        //Models
        lvl.LevelDataChunk.NumModels = reader.ReadUInt32();
        lvl.LevelDataChunk.Models = new TRModel[lvl.LevelDataChunk.NumModels];

        for (int i = 0; i < lvl.LevelDataChunk.NumModels; i++)
        {
            lvl.LevelDataChunk.Models[i] = TR2FileReadUtilities.ReadModel(reader);
        }
    }

    public static void PopulateStaticMeshes(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.NumStaticMeshes = reader.ReadUInt32();
        lvl.LevelDataChunk.StaticMeshes = new TRStaticMesh[lvl.LevelDataChunk.NumStaticMeshes];

        for (int i = 0; i < lvl.LevelDataChunk.NumStaticMeshes; i++)
        {
            lvl.LevelDataChunk.StaticMeshes[i] = TR2FileReadUtilities.ReadStaticMesh(reader);
        }
    }

    public static void VerifySPRMarker(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.SPRMarker = reader.ReadBytes(3);

        Debug.Assert(lvl.LevelDataChunk.SPRMarker[0] == 0x53);
        Debug.Assert(lvl.LevelDataChunk.SPRMarker[1] == 0x50);
        Debug.Assert(lvl.LevelDataChunk.SPRMarker[2] == 0x52);
    }

    public static void PopulateSprites(BinaryReader reader, TR4Level lvl)
    {
        //Sprite Textures
        lvl.LevelDataChunk.NumSpriteTextures = reader.ReadUInt32();
        lvl.LevelDataChunk.SpriteTextures = new TRSpriteTexture[lvl.LevelDataChunk.NumSpriteTextures];

        for (int i = 0; i < lvl.LevelDataChunk.NumSpriteTextures; i++)
        {
            lvl.LevelDataChunk.SpriteTextures[i] = TR2FileReadUtilities.ReadSpriteTexture(reader);
        }

        //Sprite Sequences
        lvl.LevelDataChunk.NumSpriteSequences = reader.ReadUInt32();
        lvl.LevelDataChunk.SpriteSequences = new TRSpriteSequence[lvl.LevelDataChunk.NumSpriteSequences];

        for (int i = 0; i < lvl.LevelDataChunk.NumSpriteSequences; i++)
        {
            lvl.LevelDataChunk.SpriteSequences[i] = TR2FileReadUtilities.ReadSpriteSequence(reader);
        }
    }

    public static void PopulateCameras(BinaryReader reader, TR4Level lvl)
    {
        //Cameras
        lvl.LevelDataChunk.NumCameras = reader.ReadUInt32();
        lvl.LevelDataChunk.Cameras = new TRCamera[lvl.LevelDataChunk.NumCameras];

        for (int i = 0; i < lvl.LevelDataChunk.NumCameras; i++)
        {
            lvl.LevelDataChunk.Cameras[i] = TR2FileReadUtilities.ReadCamera(reader);
        }

        //Flyby Cameras
        lvl.LevelDataChunk.NumFlybyCameras = reader.ReadUInt32();
        lvl.LevelDataChunk.FlybyCameras = new TR4FlyByCamera[lvl.LevelDataChunk.NumFlybyCameras];

        for (int i = 0; i < lvl.LevelDataChunk.NumFlybyCameras; i++)
        {
            lvl.LevelDataChunk.FlybyCameras[i] = TR4FileReadUtilities.ReadFlybyCamera(reader);
        }
    }

    public static void PopulateSoundSources(BinaryReader reader, TR4Level lvl)
    {
        //Sound Sources
        lvl.LevelDataChunk.NumSoundSources = reader.ReadUInt32();
        lvl.LevelDataChunk.SoundSources = new TRSoundSource[lvl.LevelDataChunk.NumSoundSources];

        for (int i = 0; i < lvl.LevelDataChunk.NumSoundSources; i++)
        {
            lvl.LevelDataChunk.SoundSources[i] = TR2FileReadUtilities.ReadSoundSource(reader);
        }
    }

    public static void PopulateBoxesOverlapsZones(BinaryReader reader, TR4Level lvl)
    {
        //Boxes
        lvl.LevelDataChunk.NumBoxes = reader.ReadUInt32();
        lvl.LevelDataChunk.Boxes = new TR2Box[lvl.LevelDataChunk.NumBoxes];

        for (int i = 0; i < lvl.LevelDataChunk.NumBoxes; i++)
        {
            lvl.LevelDataChunk.Boxes[i] = TR2FileReadUtilities.ReadBox(reader);
        }

        //Overlaps & Zones
        lvl.LevelDataChunk.NumOverlaps = reader.ReadUInt32();
        lvl.LevelDataChunk.Overlaps = new ushort[lvl.LevelDataChunk.NumOverlaps];
        lvl.LevelDataChunk.Zones = new short[10 * lvl.LevelDataChunk.NumBoxes];

        for (int i = 0; i < lvl.LevelDataChunk.NumOverlaps; i++)
        {
            lvl.LevelDataChunk.Overlaps[i] = reader.ReadUInt16();
        }

        for (int i = 0; i < lvl.LevelDataChunk.Zones.Length; i++)
        {
            lvl.LevelDataChunk.Zones[i] = reader.ReadInt16();
        }
    }

    public static void PopulateAnimatedTextures(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.NumAnimatedTextures = reader.ReadUInt32();
        lvl.LevelDataChunk.AnimatedTextures = new TRAnimatedTexture[reader.ReadUInt16()];
        for (int i = 0; i < lvl.LevelDataChunk.AnimatedTextures.Length; i++)
        {
            lvl.LevelDataChunk.AnimatedTextures[i] = TR2FileReadUtilities.ReadAnimatedTexture(reader);
        }

        //TR4+ Specific
        lvl.LevelDataChunk.AnimatedTexturesUVCount = reader.ReadByte();
    }

    public static void VerifyTEXMarker(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.TEXMarker = reader.ReadBytes(3);

        Debug.Assert(lvl.LevelDataChunk.TEXMarker[0] == 0x54);
        Debug.Assert(lvl.LevelDataChunk.TEXMarker[1] == 0x45);
        Debug.Assert(lvl.LevelDataChunk.TEXMarker[2] == 0x58);
    }

    public static void PopulateObjectTextures(BinaryReader reader, TR4Level lvl)
    {
        //Object Textures
        lvl.LevelDataChunk.NumObjectTextures = reader.ReadUInt32();
        lvl.LevelDataChunk.ObjectTextures = new TR4ObjectTexture[lvl.LevelDataChunk.NumObjectTextures];

        for (int i = 0; i < lvl.LevelDataChunk.NumObjectTextures; i++)
        {
            lvl.LevelDataChunk.ObjectTextures[i] = TR4FileReadUtilities.ReadObjectTexture(reader);
        }
    }

    public static void PopulateEntitiesAndAI(BinaryReader reader, TR4Level lvl)
    {
        //Entities
        lvl.LevelDataChunk.NumEntities = reader.ReadUInt32();
        lvl.LevelDataChunk.Entities = new TR4Entity[lvl.LevelDataChunk.NumEntities];

        for (int i = 0; i < lvl.LevelDataChunk.NumEntities; i++)
        {
            lvl.LevelDataChunk.Entities[i] = TR4FileReadUtilities.ReadEntity(reader);
        }

        //AIObjects
        lvl.LevelDataChunk.NumAIObjects = reader.ReadUInt32();
        lvl.LevelDataChunk.AIObjects = new TR4AIObject[lvl.LevelDataChunk.NumAIObjects];

        for (int i = 0; i < lvl.LevelDataChunk.NumAIObjects; i++)
        {
            lvl.LevelDataChunk.AIObjects[i] = TR4FileReadUtilities.ReadAIObject(reader);
        }
    }

    public static void PopulateDemoSoundSampleIndices(BinaryReader reader, TR4Level lvl)
    {
        //Demo Data
        lvl.LevelDataChunk.NumDemoData = reader.ReadUInt16();
        lvl.LevelDataChunk.DemoData = new byte[lvl.LevelDataChunk.NumDemoData];

        for (int i = 0; i < lvl.LevelDataChunk.NumDemoData; i++)
        {
            lvl.LevelDataChunk.DemoData[i] = reader.ReadByte();
        }

        //Sound Map (370 shorts) & Sound Details
        lvl.LevelDataChunk.SoundMap = new short[370];

        for (int i = 0; i < lvl.LevelDataChunk.SoundMap.Length; i++)
        {
            lvl.LevelDataChunk.SoundMap[i] = reader.ReadInt16();
        }

        lvl.LevelDataChunk.NumSoundDetails = reader.ReadUInt32();
        lvl.LevelDataChunk.SoundDetails = new TR3SoundDetails[lvl.LevelDataChunk.NumSoundDetails];

        for (int i = 0; i < lvl.LevelDataChunk.NumSoundDetails; i++)
        {
            lvl.LevelDataChunk.SoundDetails[i] = TR3FileReadUtilities.ReadSoundDetails(reader);
        }

        //Samples
        lvl.LevelDataChunk.NumSampleIndices = reader.ReadUInt32();
        lvl.LevelDataChunk.SampleIndices = new uint[lvl.LevelDataChunk.NumSampleIndices];

        for (int i = 0; i < lvl.LevelDataChunk.NumSampleIndices; i++)
        {
            lvl.LevelDataChunk.SampleIndices[i] = reader.ReadUInt32();
        }
    }

    public static void VerifyLevelDataFinalSeperator(BinaryReader reader, TR4Level lvl)
    {
        lvl.LevelDataChunk.Seperator = reader.ReadBytes(6);

        Debug.Assert(lvl.LevelDataChunk.Seperator[0] == 0x00);
        Debug.Assert(lvl.LevelDataChunk.Seperator[1] == 0x00);
        Debug.Assert(lvl.LevelDataChunk.Seperator[2] == 0x00);
        Debug.Assert(lvl.LevelDataChunk.Seperator[3] == 0x00);
        Debug.Assert(lvl.LevelDataChunk.Seperator[4] == 0x00);
        Debug.Assert(lvl.LevelDataChunk.Seperator[5] == 0x00);
    }

    private static TR3RoomData ConvertToRoomData(TR4Room room)
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

    private static TR4RoomLight ReadRoomLight(BinaryReader reader)
    {
        return new TR4RoomLight
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Colour = new TRColour
            {
                Red = reader.ReadByte(),
                Green = reader.ReadByte(),
                Blue = reader.ReadByte()
            },
            LightType = reader.ReadByte(),
            Unknown = reader.ReadByte(),
            Intensity = reader.ReadByte(),
            In = reader.ReadSingle(),
            Out = reader.ReadSingle(),
            Length = reader.ReadSingle(),
            CutOff = reader.ReadSingle(),
            Dx = reader.ReadSingle(),
            Dy = reader.ReadSingle(),
            Dz = reader.ReadSingle()
        };
    }

    public static TR4Mesh[] ConstructMeshData(uint[] meshPointers, ushort[] rawMeshData)
    {
        byte[] target = new byte[rawMeshData.Length * 2];
        Buffer.BlockCopy(rawMeshData, 0, target, 0, target.Length);

        // The mesh pointer list can contain duplicates so we must make
        // sure to iterate over distinct values only
        meshPointers = meshPointers.Distinct().ToArray();

        List<TR4Mesh> meshes = new();

        using (MemoryStream ms = new(target))
        using (BinaryReader br = new(ms))
        {
            for (int i = 0; i < meshPointers.Length; i++)
            {
                TR4Mesh mesh = new();
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
                mesh.TexturedRectangles = new TR4MeshFace4[mesh.NumTexturedRectangles];
                for (int j = 0; j < mesh.NumTexturedRectangles; j++)
                {
                    mesh.TexturedRectangles[j] = TR4FileReadUtilities.ReadTR4MeshFace4(br);
                }

                //Textured Triangles
                mesh.NumTexturedTriangles = br.ReadInt16();
                mesh.TexturedTriangles = new TR4MeshFace3[mesh.NumTexturedTriangles];
                for (int j = 0; j < mesh.NumTexturedTriangles; j++)
                {
                    mesh.TexturedTriangles[j] = TR4FileReadUtilities.ReadTR4MeshFace3(br);
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

    public static TR4MeshFace4 ReadTR4MeshFace4(BinaryReader reader)
    {
        return new TR4MeshFace4
        {
            Vertices = ReadVertices(reader, 4),
            Texture = reader.ReadUInt16(),
            Effects = reader.ReadUInt16()
        };
    }

    public static TR4MeshFace3 ReadTR4MeshFace3(BinaryReader reader)
    {
        return new TR4MeshFace3
        {
            Vertices = ReadVertices(reader, 3),
            Texture = reader.ReadUInt16(),
            Effects = reader.ReadUInt16()
        };
    }

    private static ushort[] ReadVertices(BinaryReader reader, int count)
    {
        ushort[] vertices = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            vertices[i] = reader.ReadUInt16();
        }
        return vertices;
    }

    public static TR4Animation ReadAnimation(BinaryReader reader)
    {
        return new TR4Animation
        {
            FrameOffset = reader.ReadUInt32(),
            FrameRate = reader.ReadByte(),
            FrameSize = reader.ReadByte(),
            StateID = reader.ReadUInt16(),
            Speed = new FixedFloat32
            {
                Whole = reader.ReadInt16(),
                Fraction = reader.ReadUInt16()
            },
            Accel = new FixedFloat32
            {
                Whole = reader.ReadInt16(),
                Fraction = reader.ReadUInt16()
            },
            SpeedLateral = new FixedFloat32
            {
                Whole = reader.ReadInt16(),
                Fraction = reader.ReadUInt16()
            },
            AccelLateral = new FixedFloat32
            {
                Whole = reader.ReadInt16(),
                Fraction = reader.ReadUInt16()
            },
            FrameStart = reader.ReadUInt16(),
            FrameEnd = reader.ReadUInt16(),
            NextAnimation = reader.ReadUInt16(),
            NextFrame = reader.ReadUInt16(),
            NumStateChanges = reader.ReadUInt16(),
            StateChangeOffset = reader.ReadUInt16(),
            NumAnimCommands = reader.ReadUInt16(),
            AnimCommand = reader.ReadUInt16()
        };
    }

    public static TR4FlyByCamera ReadFlybyCamera(BinaryReader reader)
    {
        return new TR4FlyByCamera
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Dx = reader.ReadInt32(),
            Dy = reader.ReadInt32(),
            Dz = reader.ReadInt32(),
            Sequence = reader.ReadByte(),
            Index = reader.ReadByte(),
            FOV = reader.ReadUInt16(),
            Roll = reader.ReadInt16(),
            Timer = reader.ReadUInt16(),
            Speed = reader.ReadUInt16(),
            Flags = reader.ReadUInt16(),
            RoomID = reader.ReadUInt32()
        };
    }

    public static TR4ObjectTexture ReadObjectTexture(BinaryReader reader)
    {
        return new TR4ObjectTexture()
        {
            Attribute = reader.ReadUInt16(),
            TileAndFlag = reader.ReadUInt16(),
            NewFlags = reader.ReadUInt16(),
            Vertices = new TRObjectTextureVert[]
            {
                new TRObjectTextureVert
                {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new TRObjectTextureVert
                {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new TRObjectTextureVert
                {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new TRObjectTextureVert
                {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                }
            },
            OriginalU = reader.ReadUInt32(),
            OriginalV = reader.ReadUInt32(),
            WidthMinusOne = reader.ReadUInt32(),
            HeightMinusOne = reader.ReadUInt32()
        };
    }

    public static TR4Entity ReadEntity(BinaryReader reader)
    {
        return new TR4Entity()
        {
            TypeID = reader.ReadInt16(),
            Room = reader.ReadInt16(),
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Angle = reader.ReadInt16(),
            Intensity = reader.ReadInt16(),
            OCB = reader.ReadInt16(),
            Flags = reader.ReadUInt16()
        };
    }

    public static TR4AIObject ReadAIObject(BinaryReader reader)
    {
        return new TR4AIObject()
        {
            TypeID = reader.ReadUInt16(),
            Room = reader.ReadUInt16(),
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            OCB = reader.ReadInt16(),
            Flags = reader.ReadUInt16(),
            Angle = reader.ReadInt32()
        };
    }
}
