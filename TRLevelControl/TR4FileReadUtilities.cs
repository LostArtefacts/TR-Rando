using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR4FileReadUtilities
{
    public static readonly string TEXMarker = "TEX";

    public static void PopulateRooms(TRLevelReader reader, TR4Level lvl)
    {
        ushort numRooms = reader.ReadUInt16();
        lvl.Rooms = new();

        for (int i = 0; i < numRooms; i++)
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
            };
            lvl.Rooms.Add(room);

            uint numWords = reader.ReadUInt32();
            room.Mesh = ConvertToRoomData(reader.ReadUInt16s(numWords));

            //Portals
            ushort numPortals = reader.ReadUInt16();
            room.Portals = new();
            for (int j = 0; j < numPortals; j++)
            {
                room.Portals.Add(TR2FileReadUtilities.ReadRoomPortal(reader));
            }

            //Sectors
            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = new();
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors.Add(TR2FileReadUtilities.ReadRoomSector(reader));
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.LightMode = reader.ReadInt16();
            ushort numLights = reader.ReadUInt16();
            room.Lights = new();
            for (int j = 0; j < numLights; j++)
            {
                room.Lights.Add(ReadRoomLight(reader));
            }

            //Static meshes
            ushort numStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new();
            for (int j = 0; j < numStaticMeshes; j++)
            {
                room.StaticMeshes.Add(ReadRoomStaticMesh(reader));
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();
            room.WaterScheme = reader.ReadByte();
            room.ReverbInfo = reader.ReadByte();
            room.Filler = reader.ReadByte();
        }
    }

    public static TR4RoomMesh ConvertToRoomData(ushort[] rawData)
    {
        // This approach is temporarily retained

        TR4RoomMesh roomData = new()
        {
            Vertices = new()
        };

        int offset = 0;
        ushort count = rawData[offset++];
        for (int j = 0; j < count; j++)
        {
            roomData.Vertices.Add(new()
            {
                Vertex = new()
                {
                    X = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Y = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Z = UnsafeConversions.UShortToShort(rawData[offset++]),
                },
                Lighting = UnsafeConversions.UShortToShort(rawData[offset++]),
                Attributes = rawData[offset++],
                Colour = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Rectangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Rectangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Triangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Triangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Sprites = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Sprites.Add(new()
            {
                Vertex = UnsafeConversions.UShortToShort(rawData[offset++]),
                Texture = UnsafeConversions.UShortToShort(rawData[offset++]),
            });
        }

        Debug.Assert(offset == rawData.Length);

        return roomData;
    }

    public static TR3RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
    {
        return new TR3RoomStaticMesh
        {
            X = reader.ReadUInt32(),
            Y = reader.ReadUInt32(),
            Z = reader.ReadUInt32(),
            Rotation = reader.ReadUInt16(),
            Colour = reader.ReadUInt16(),
            Unused = reader.ReadUInt16(),
            MeshID = reader.ReadUInt16()
        };
    }

    public static void PopulateFloordata(BinaryReader reader, TR4Level lvl)
    {
        uint numFloorData = reader.ReadUInt32();
        lvl.FloorData = new();

        for (int i = 0; i < numFloorData; i++)
        {
            lvl.FloorData.Add(reader.ReadUInt16());
        }
    }

    public static void PopulateCameras(BinaryReader reader, TR4Level lvl)
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
            lvl.FlybyCameras.Add(ReadFlybyCamera(reader));
        }
    }

    public static void PopulateSoundSources(BinaryReader reader, TR4Level lvl)
    {
        //Sound Sources
        uint numSoundSources = reader.ReadUInt32();
        lvl.SoundSources = new();

        for (int i = 0; i < numSoundSources; i++)
        {
            lvl.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }
    }

    public static void PopulateBoxesOverlapsZones(BinaryReader reader, TR4Level lvl)
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

    public static void PopulateAnimatedTextures(BinaryReader reader, TR4Level lvl)
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

    public static void PopulateObjectTextures(BinaryReader reader, TR4Level lvl)
    {
        string texMarker = new(reader.ReadChars(TEXMarker.Length));
        Debug.Assert(texMarker == TEXMarker);

        uint numObjectTextures = reader.ReadUInt32();
        lvl.ObjectTextures = new();

        for (int i = 0; i < numObjectTextures; i++)
        {
            lvl.ObjectTextures.Add(ReadObjectTexture(reader));
        }
    }

    public static void PopulateEntitiesAndAI(TRLevelReader reader, TR4Level lvl)
    {
        //Entities
        uint numEntities = reader.ReadUInt32();
        lvl.Entities = reader.ReadTR4Entities(numEntities);

        //AIObjects
        numEntities = reader.ReadUInt32();
        lvl.AIEntities = reader.ReadTR4AIEntities(numEntities);
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
            HeightMinusOne = reader.ReadUInt32()
        };
    }
}
