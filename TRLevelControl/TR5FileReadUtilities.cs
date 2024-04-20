using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR5FileReadUtilities
{
    public static readonly string SPRMarker = "SPR\0";
    public static readonly string TEXMarker = "TEX\0";

    public static void PopulateRooms(BinaryReader reader, TR5Level lvl)
    {
        uint numRooms = reader.ReadUInt32();
        lvl.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TR5Room room = new()
            {
                XELALandmark = reader.ReadBytes(4)
            };
            lvl.Rooms.Add(room);

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
        lvl.FloorData = new();

        for (int i = 0; i < numFloorData; i++)
        {
            lvl.FloorData.Add(reader.ReadUInt16());
        }
    }

    public static void PopulateAnimations(BinaryReader reader, TR5Level lvl)
    {
        //Animations
        uint numAnimations = reader.ReadUInt32();
        lvl.Animations = new();
        for (int i = 0; i < numAnimations; i++)
        {
            lvl.Animations.Add(TR4FileReadUtilities.ReadAnimation(reader));
        }

        //State Changes
        uint numStateChanges = reader.ReadUInt32();
        lvl.StateChanges = new();
        for (int i = 0; i < numStateChanges; i++)
        {
            lvl.StateChanges.Add(TR2FileReadUtilities.ReadStateChange(reader));
        }

        //Animation Dispatches
        uint numAnimDispatches = reader.ReadUInt32();
        lvl.AnimDispatches = new();
        for (int i = 0; i < numAnimDispatches; i++)
        {
            lvl.AnimDispatches.Add(TR2FileReadUtilities.ReadAnimDispatch(reader));
        }

        //Animation Commands
        uint numAnimCommands = reader.ReadUInt32();
        lvl.AnimCommands = new();
        for (int i = 0; i < numAnimCommands; i++)
        {
            lvl.AnimCommands.Add(TR2FileReadUtilities.ReadAnimCommand(reader));
        }
    }

    public static void PopulateMeshTreesFramesModels(BinaryReader reader, TR5Level lvl)
    {
        //Mesh Trees
        uint numMeshTrees = reader.ReadUInt32() / 4;
        lvl.MeshTrees = new();
        for (int i = 0; i < numMeshTrees; i++)
        {
            lvl.MeshTrees.Add(TR2FileReadUtilities.ReadMeshTreeNode(reader));
        }

        //Frames
        uint numFrames = reader.ReadUInt32();
        lvl.Frames = new();
        for (int i = 0; i < numFrames; i++)
        {
            lvl.Frames.Add(reader.ReadUInt16());
        }

        //Models
        uint numModels = reader.ReadUInt32();
        lvl.Models = new();
        for (int i = 0; i < numModels; i++)
        {
            lvl.Models.Add(ReadTR5Model(reader));
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
        lvl.StaticMeshes = new();

        for (int i = 0; i < numStaticMeshes; i++)
        {
            lvl.StaticMeshes.Add(TR2FileReadUtilities.ReadStaticMesh(reader));
        }
    }

    public static void VerifySPRMarker(BinaryReader reader)
    {
        string sprMarker = new(reader.ReadChars(SPRMarker.Length));
        Debug.Assert(sprMarker == SPRMarker);
    }

    public static void PopulateSprites(BinaryReader reader, TR5Level lvl)
    {
        uint numSpriteTextures = reader.ReadUInt32();
        lvl.SpriteTextures = new();

        for (int i = 0; i < numSpriteTextures; i++)
        {
            lvl.SpriteTextures.Add(TR2FileReadUtilities.ReadSpriteTexture(reader));
        }

        uint numSpriteSequences = reader.ReadUInt32();
        lvl.SpriteSequences = new();

        for (int i = 0; i < numSpriteSequences; i++)
        {
            lvl.SpriteSequences.Add(TR2FileReadUtilities.ReadSpriteSequence(reader));
        }
    }

    public static void PopulateCameras(BinaryReader reader, TR5Level lvl)
    {
        //Cameras
        uint numCameras = reader.ReadUInt32();
        lvl.Cameras = new();

        for (int i = 0; i < numCameras; i++)
        {
            lvl.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        //Flyby Cameras
        uint numFlybyCameras = reader.ReadUInt32();
        lvl.FlybyCameras = new();

        for (int i = 0; i < numFlybyCameras; i++)
        {
            lvl.FlybyCameras.Add(TR4FileReadUtilities.ReadFlybyCamera(reader));
        }
    }

    public static void PopulateSoundSources(BinaryReader reader, TR5Level lvl)
    {
        //Sound Sources
        uint numSoundSources = reader.ReadUInt32();
        lvl.SoundSources = new();

        for (int i = 0; i < numSoundSources; i++)
        {
            lvl.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }
    }

    public static void PopulateBoxesOverlapsZones(BinaryReader reader, TR5Level lvl)
    {
        //Boxes
        uint numBoxes = reader.ReadUInt32();
        lvl.Boxes = new();

        for (int i = 0; i < numBoxes; i++)
        {
            lvl.Boxes.Add(TR2FileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        lvl.Overlaps = new();
        short[] zones = new short[10 * numBoxes];

        for (int i = 0; i < numOverlaps; i++)
        {
            lvl.Overlaps.Add(reader.ReadUInt16());
        }

        for (int i = 0; i < zones.Length; i++)
        {
            zones[i] = reader.ReadInt16();
        }
        lvl.Zones = new(zones);
    }

    public static void PopulateAnimatedTextures(BinaryReader reader, TR5Level lvl)
    {
        reader.ReadUInt32(); // Total count of ushorts
        ushort numGroups = reader.ReadUInt16();
        lvl.AnimatedTextures = new();
        for (int i = 0; i < numGroups; i++)
        {
            lvl.AnimatedTextures.Add(TR2FileReadUtilities.ReadAnimatedTexture(reader));
        }

        //TR4+ Specific
        lvl.AnimatedTexturesUVCount = reader.ReadByte();
    }

    public static void VerifyTEXMarker(BinaryReader reader)
    {
        string texMarker = new(reader.ReadChars(TEXMarker.Length));
        Debug.Assert(texMarker == TEXMarker);
    }

    public static void PopulateObjectTextures(BinaryReader reader, TR5Level lvl)
    {
        uint numObjectTextures = reader.ReadUInt32();
        lvl.ObjectTextures = new();

        for (int i = 0; i < numObjectTextures; i++)
        {
            lvl.ObjectTextures.Add(ReadTR5ObjectTexture(reader));
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
        lvl.Entities = reader.ReadTR5Entities(numEntities);

        //AIObjects
        numEntities = reader.ReadUInt32();
        lvl.AIEntities = reader.ReadTR5AIEntities(numEntities);
    }

    public static void PopulateDemoSoundSampleIndices(BinaryReader reader, TR5Level lvl)
    {
        ushort numDemoData = reader.ReadUInt16();
        lvl.DemoData = reader.ReadBytes(numDemoData);

        //Sound Map (370 shorts) & Sound Details
        lvl.SoundMap = new short[450];

        for (int i = 0; i < lvl.SoundMap.Length; i++)
        {
            lvl.SoundMap[i] = reader.ReadInt16();
        }

        uint numSoundDetails = reader.ReadUInt32();
        lvl.SoundDetails = new();

        for (int i = 0; i < numSoundDetails; i++)
        {
            lvl.SoundDetails.Add(TR4FileReadUtilities.ReadSoundDetails(reader));
        }

        uint numSampleIndices = reader.ReadUInt32();
        lvl.SampleIndices = new();

        for (int i = 0; i < numSampleIndices; i++)
        {
            lvl.SampleIndices.Add(reader.ReadUInt32());
        }
    }
}
