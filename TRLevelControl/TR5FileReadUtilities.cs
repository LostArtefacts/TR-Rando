using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR5FileReadUtilities
{
    public static void PopulateRooms(BinaryReader reader, TR5Level lvl)
    {
        lvl.LevelDataChunk.Unused = reader.ReadUInt32();
        uint numRooms = reader.ReadUInt32();
        lvl.LevelDataChunk.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR5Room room = new()
            {
                XELALandmark = reader.ReadBytes(4)
            };
            lvl.LevelDataChunk.Rooms.Add(room);

            Debug.Assert(room.XELALandmark[0] == 'X');
            Debug.Assert(room.XELALandmark[1] == 'E');
            Debug.Assert(room.XELALandmark[2] == 'L');
            Debug.Assert(room.XELALandmark[3] == 'A');

            room.RoomDataSize = reader.ReadUInt32();
            long lastPosition = reader.BaseStream.Position;
            room.Seperator = reader.ReadUInt32();
            room.EndSDOffset = reader.ReadUInt32();
            room.StartSDOffset = reader.ReadUInt32();
            room.Seperator2 = reader.ReadUInt32();
            room.EndPortalOffset = reader.ReadUInt32();

            TR5RoomInfo info = new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                YBottom = reader.ReadInt32(),
                YTop = reader.ReadInt32()
            };

            room.Info = info;

            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.RoomColourARGB = reader.ReadUInt32();
            room.NumLights = reader.ReadUInt16();
            room.NumStaticMeshes = reader.ReadUInt16();

            room.Reverb = reader.ReadByte();
            room.AlternateGroup = reader.ReadByte();
            room.WaterScheme = reader.ReadUInt16();

            room.Filler = new uint[2];
            room.Filler[0] = reader.ReadUInt32();
            room.Filler[1] = reader.ReadUInt32();

            room.Seperator3 = new uint[2];
            room.Seperator3[0] = reader.ReadUInt32();
            room.Seperator3[1] = reader.ReadUInt32();

            room.Filler2 = reader.ReadUInt32();
            room.AlternateRoom = reader.ReadUInt16();
            room.Flags = reader.ReadUInt16();

            room.Unknown1 = reader.ReadUInt32();
            room.Unknown2 = reader.ReadUInt32();
            room.Unknown3 = reader.ReadUInt32();
            room.Seperator4 = reader.ReadUInt32();
            room.Unknown4 = reader.ReadUInt16();
            room.Unknown5 = reader.ReadUInt16();

            room.RoomX = reader.ReadSingle();
            room.RoomY = reader.ReadSingle();
            room.RoomZ = reader.ReadSingle();

            room.Seperator5 = new uint[4];
            room.Seperator5[0] = reader.ReadUInt32();
            room.Seperator5[1] = reader.ReadUInt32();
            room.Seperator5[2] = reader.ReadUInt32();
            room.Seperator5[3] = reader.ReadUInt32();

            room.Seperator6 = reader.ReadUInt32();
            room.Seperator7 = reader.ReadUInt32();

            room.NumRoomTriangles = reader.ReadUInt32();
            room.NumRoomRectangles = reader.ReadUInt32();

            room.RoomLightsPtr = reader.ReadUInt32();
            room.RoomFogBulbsPtr = reader.ReadUInt32();

            room.NumLights2 = reader.ReadUInt32();
            room.NumFogBulbs = reader.ReadUInt32();

            room.RoomYTop = reader.ReadSingle();
            room.RoomYBottom = reader.ReadSingle();

            room.NumLayers = reader.ReadUInt32();
            room.LayersPtr = reader.ReadUInt32();
            room.VerticesDataSize = reader.ReadUInt32();
            room.PolyOffset = reader.ReadUInt32();
            room.PolyOffset2 = reader.ReadUInt32();
            room.NumVertices = reader.ReadUInt32();

            room.Seperator8 = new uint[4];
            room.Seperator8[0] = reader.ReadUInt32();
            room.Seperator8[1] = reader.ReadUInt32();
            room.Seperator8[2] = reader.ReadUInt32();
            room.Seperator8[3] = reader.ReadUInt32();

            //Record the stream pointer after the header
            long afterhdr = reader.BaseStream.Position;

            //Room data is currently read as bytes.
            //To modify in future we will need to parse properly.
            TR5RoomData data = new()
            {
                AsBytes = reader.ReadBytes((int)(lastPosition + room.RoomDataSize) - (int)afterhdr)
            };

            PopulateLightsBulbsAndSectors(room, data);

            room.RoomData = data;
        }
    }

    private static void PopulateLightsBulbsAndSectors(TR5Room room, TR5RoomData data)
    {
        if (data.AsBytes != null)
        {
            using MemoryStream stream = new(data.AsBytes, false);
            using BinaryReader rdatareader = new(stream);
            data.Lights = new TR5RoomLight[room.NumLights];
            data.FogBulbs = new TR5FogBulb[room.NumFogBulbs];
            data.SectorList = new TRRoomSector[room.NumXSectors * room.NumZSectors];

            for (int i = 0; i < room.NumLights; i++)
            {
                data.Lights[i] = TR5FileReadUtilities.ReadRoomLight(rdatareader);
            }

            for (int i = 0; i < room.NumFogBulbs; i++)
            {
                data.FogBulbs[i] = TR5FileReadUtilities.ReadRoomBulbs(rdatareader);
            }

            for (int i = 0; i < room.NumXSectors * room.NumZSectors; i++)
            {
                data.SectorList[i] = TR2FileReadUtilities.ReadRoomSector(rdatareader);
            }
        }
    }

    private static TR5RoomLight ReadRoomLight(BinaryReader r)
    {
        return new TR5RoomLight()
        {
            X = r.ReadSingle(),
            Y = r.ReadSingle(),
            Z = r.ReadSingle(),
            R = r.ReadSingle(),
            G = r.ReadSingle(),
            B = r.ReadSingle(),
            Seperator = r.ReadUInt32(),
            In = r.ReadSingle(),
            Out = r.ReadSingle(),
            RadIn = r.ReadSingle(),
            RadOut = r.ReadSingle(),
            Range = r.ReadSingle(),
            DX = r.ReadSingle(),
            DY = r.ReadSingle(),
            DZ = r.ReadSingle(),
            X2 = r.ReadInt32(),
            Y2 = r.ReadInt32(),
            Z2 = r.ReadInt32(),
            DX2 = r.ReadInt32(),
            DY2 = r.ReadInt32(),
            DZ2 = r.ReadInt32(),
            LightType = r.ReadByte(),
            Filler = r.ReadBytes(3)
        };
    }

    private static TR5FogBulb ReadRoomBulbs(BinaryReader r)
    {
        return new TR5FogBulb()
        {
            X = r.ReadSingle(),
            Y = r.ReadSingle(),
            Z = r.ReadSingle(),
            R = r.ReadSingle(),
            G = r.ReadSingle(),
            B = r.ReadSingle(),
            Seperator = r.ReadUInt32(),
            In = r.ReadSingle(),
            Out = r.ReadSingle()
        };
    }

    public static void PopulateFloordata(BinaryReader reader, TR5Level lvl)
    {
        uint numFloorData = reader.ReadUInt32();
        lvl.LevelDataChunk.FloorData = new();

        for (int i = 0; i < numFloorData; i++)
        {
            lvl.LevelDataChunk.FloorData.Add(reader.ReadUInt16());
        }
    }

    public static void PopulateMeshes(BinaryReader reader, TR5Level lvl)
    {
        uint numMeshData = reader.ReadUInt32();
        ushort[] rawMeshData = new ushort[numMeshData];

        for (int i = 0; i < numMeshData; i++)
        {
            rawMeshData[i] = reader.ReadUInt16();
        }

        uint numMeshPointers = reader.ReadUInt32();
        lvl.LevelDataChunk.MeshPointers = new();

        for (int i = 0; i < numMeshPointers; i++)
        {
            lvl.LevelDataChunk.MeshPointers.Add(reader.ReadUInt32());
        }

        lvl.LevelDataChunk.Meshes = TR4FileReadUtilities.ConstructMeshData(lvl.LevelDataChunk.MeshPointers, rawMeshData);
    }

    public static void PopulateAnimations(BinaryReader reader, TR5Level lvl)
    {
        //Animations
        uint numAnimations = reader.ReadUInt32();
        lvl.LevelDataChunk.Animations = new();
        for (int i = 0; i < numAnimations; i++)
        {
            lvl.LevelDataChunk.Animations.Add(TR4FileReadUtilities.ReadAnimation(reader));
        }

        //State Changes
        uint numStateChanges = reader.ReadUInt32();
        lvl.LevelDataChunk.StateChanges = new();
        for (int i = 0; i < numStateChanges; i++)
        {
            lvl.LevelDataChunk.StateChanges.Add(TR2FileReadUtilities.ReadStateChange(reader));
        }

        //Animation Dispatches
        uint numAnimDispatches = reader.ReadUInt32();
        lvl.LevelDataChunk.AnimDispatches = new();
        for (int i = 0; i < numAnimDispatches; i++)
        {
            lvl.LevelDataChunk.AnimDispatches.Add(TR2FileReadUtilities.ReadAnimDispatch(reader));
        }

        //Animation Commands
        uint numAnimCommands = reader.ReadUInt32();
        lvl.LevelDataChunk.AnimCommands = new();
        for (int i = 0; i < numAnimCommands; i++)
        {
            lvl.LevelDataChunk.AnimCommands.Add(TR2FileReadUtilities.ReadAnimCommand(reader));
        }
    }

    public static void PopulateMeshTreesFramesModels(BinaryReader reader, TR5Level lvl)
    {
        //Mesh Trees
        uint numMeshTrees = reader.ReadUInt32() / 4;
        lvl.LevelDataChunk.MeshTrees = new();
        for (int i = 0; i < numMeshTrees; i++)
        {
            lvl.LevelDataChunk.MeshTrees.Add(TR2FileReadUtilities.ReadMeshTreeNode(reader));
        }

        //Frames
        uint numFrames = reader.ReadUInt32();
        lvl.LevelDataChunk.Frames = new();
        for (int i = 0; i < numFrames; i++)
        {
            lvl.LevelDataChunk.Frames.Add(reader.ReadUInt16());
        }

        //Models
        uint numModels = reader.ReadUInt32();
        lvl.LevelDataChunk.Models = new();
        for (int i = 0; i < numModels; i++)
        {
            lvl.LevelDataChunk.Models.Add(ReadTR5Model(reader));
        }
    }

    private static TR5Model ReadTR5Model(BinaryReader reader)
    {
        return new TR5Model()
        {
            ID = reader.ReadUInt32(),
            NumMeshes = reader.ReadUInt16(),
            StartingMesh = reader.ReadUInt16(),
            MeshTree = reader.ReadUInt32(),
            FrameOffset = reader.ReadUInt32(),
            Animation = reader.ReadUInt16(),
            Filler = reader.ReadUInt16()
        };
    }

    public static void PopulateStaticMeshes(BinaryReader reader, TR5Level lvl)
    {
        uint numStaticMeshes = reader.ReadUInt32();
        lvl.LevelDataChunk.StaticMeshes = new();

        for (int i = 0; i < numStaticMeshes; i++)
        {
            lvl.LevelDataChunk.StaticMeshes.Add(TR2FileReadUtilities.ReadStaticMesh(reader));
        }
    }

    public static void VerifySPRMarker(BinaryReader reader, TR5Level lvl)
    {
        lvl.LevelDataChunk.SPRMarker = reader.ReadBytes(4);

        Debug.Assert(lvl.LevelDataChunk.SPRMarker[0] == 0x53);
        Debug.Assert(lvl.LevelDataChunk.SPRMarker[1] == 0x50);
        Debug.Assert(lvl.LevelDataChunk.SPRMarker[2] == 0x52);
        Debug.Assert(lvl.LevelDataChunk.SPRMarker[3] == 0x00);
    }

    public static void PopulateSprites(BinaryReader reader, TR5Level lvl)
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

    public static void PopulateCameras(BinaryReader reader, TR5Level lvl)
    {
        //Cameras
        uint numCameras = reader.ReadUInt32();
        lvl.LevelDataChunk.Cameras = new();

        for (int i = 0; i < numCameras; i++)
        {
            lvl.LevelDataChunk.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        //Flyby Cameras
        uint numFlybyCameras = reader.ReadUInt32();
        lvl.LevelDataChunk.FlybyCameras = new();

        for (int i = 0; i < numFlybyCameras; i++)
        {
            lvl.LevelDataChunk.FlybyCameras.Add(TR4FileReadUtilities.ReadFlybyCamera(reader));
        }
    }

    public static void PopulateSoundSources(BinaryReader reader, TR5Level lvl)
    {
        //Sound Sources
        uint numSoundSources = reader.ReadUInt32();
        lvl.LevelDataChunk.SoundSources = new();

        for (int i = 0; i < numSoundSources; i++)
        {
            lvl.LevelDataChunk.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }
    }

    public static void PopulateBoxesOverlapsZones(BinaryReader reader, TR5Level lvl)
    {
        //Boxes
        uint numBoxes = reader.ReadUInt32();
        lvl.LevelDataChunk.Boxes = new();

        for (int i = 0; i < numBoxes; i++)
        {
            lvl.LevelDataChunk.Boxes.Add(TR2FileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        lvl.LevelDataChunk.Overlaps = new();
        short[] zones = new short[10 * numBoxes];

        for (int i = 0; i < numOverlaps; i++)
        {
            lvl.LevelDataChunk.Overlaps.Add(reader.ReadUInt16());
        }

        for (int i = 0; i < zones.Length; i++)
        {
            zones[i] = reader.ReadInt16();
        }
        lvl.LevelDataChunk.Zones = new(zones);
    }

    public static void PopulateAnimatedTextures(BinaryReader reader, TR5Level lvl)
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

    public static void VerifyTEXMarker(BinaryReader reader, TR5Level lvl)
    {
        lvl.LevelDataChunk.TEXMarker = reader.ReadBytes(4);

        Debug.Assert(lvl.LevelDataChunk.TEXMarker[0] == 0x54);
        Debug.Assert(lvl.LevelDataChunk.TEXMarker[1] == 0x45);
        Debug.Assert(lvl.LevelDataChunk.TEXMarker[2] == 0x58);
        Debug.Assert(lvl.LevelDataChunk.TEXMarker[3] == 0x00);
    }

    public static void PopulateObjectTextures(BinaryReader reader, TR5Level lvl)
    {
        uint numObjectTextures = reader.ReadUInt32();
        lvl.LevelDataChunk.ObjectTextures = new();

        for (int i = 0; i < numObjectTextures; i++)
        {
            lvl.LevelDataChunk.ObjectTextures.Add(ReadTR5ObjectTexture(reader));
        }
    }

    private static TR5ObjectTexture ReadTR5ObjectTexture(BinaryReader reader)
    {
        return new TR5ObjectTexture()
        {
            Attribute = reader.ReadUInt16(),
            TileAndFlag = reader.ReadUInt16(),
            NewFlags = reader.ReadUInt16(),
            Vertices = new TRObjectTextureVert[]
            {
                new() {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new() {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new() {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                },

                new() {
                    XCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() },
                    YCoordinate = new FixedFloat16 { Whole = reader.ReadByte(), Fraction = reader.ReadByte() }
                }
            },
            OriginalU = reader.ReadUInt32(),
            OriginalV = reader.ReadUInt32(),
            WidthMinusOne = reader.ReadUInt32(),
            HeightMinusOne = reader.ReadUInt32(),
            Filler = reader.ReadUInt16()
        };
    }

    public static void PopulateEntitiesAndAI(TRLevelReader reader, TR5Level lvl)
    {
        //Entities
        uint numEntities = reader.ReadUInt32();
        lvl.LevelDataChunk.Entities = reader.ReadTR5Entities(numEntities);

        //AIObjects
        numEntities = reader.ReadUInt32();
        lvl.LevelDataChunk.AIEntities = reader.ReadTR5AIEntities(numEntities);
    }

    public static void PopulateDemoSoundSampleIndices(BinaryReader reader, TR5Level lvl)
    {
        //Demo Data
        lvl.LevelDataChunk.NumDemoData = reader.ReadUInt16();
        lvl.LevelDataChunk.DemoData = new byte[lvl.LevelDataChunk.NumDemoData];

        for (int i = 0; i < lvl.LevelDataChunk.NumDemoData; i++)
        {
            lvl.LevelDataChunk.DemoData[i] = reader.ReadByte();
        }

        //Sound Map (370 shorts) & Sound Details
        lvl.LevelDataChunk.SoundMap = new short[450];

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

    public static void VerifyLevelDataFinalSeperator(BinaryReader reader, TR5Level lvl)
    {
        lvl.LevelDataChunk.Seperator = reader.ReadBytes(6);

        Debug.Assert(lvl.LevelDataChunk.Seperator[0] == 0xCD);
        Debug.Assert(lvl.LevelDataChunk.Seperator[1] == 0xCD);
        Debug.Assert(lvl.LevelDataChunk.Seperator[2] == 0xCD);
        Debug.Assert(lvl.LevelDataChunk.Seperator[3] == 0xCD);
        Debug.Assert(lvl.LevelDataChunk.Seperator[4] == 0xCD);
        Debug.Assert(lvl.LevelDataChunk.Seperator[5] == 0xCD);
    }
}
