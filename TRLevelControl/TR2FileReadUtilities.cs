using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR2FileReadUtilities
{
    public static TRRoomPortal ReadRoomPortal(BinaryReader reader)
    {
        return new TRRoomPortal
        {
            AdjoiningRoom = reader.ReadUInt16(),

            Normal = new TRVertex
            {
                X = reader.ReadInt16(),
                Y = reader.ReadInt16(),
                Z = reader.ReadInt16()
            },

            Vertices = new TRVertex[]
            {
                new() { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                new() { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                new() { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
                new() { X = reader.ReadInt16(), Y = reader.ReadInt16(), Z = reader.ReadInt16() },
            }
        };
    }

    public static TRRoomSector ReadRoomSector(BinaryReader reader)
    {
        return new TRRoomSector
        {
            FDIndex = reader.ReadUInt16(),
            BoxIndex = reader.ReadUInt16(),
            RoomBelow = reader.ReadByte(),
            Floor = reader.ReadSByte(),
            RoomAbove = reader.ReadByte(),
            Ceiling = reader.ReadSByte()
        };
    }

    public static TR2RoomLight ReadRoomLight(BinaryReader reader)
    {
        return new TR2RoomLight
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Intensity1 = reader.ReadUInt16(),
            Intensity2 = reader.ReadUInt16(),
            Fade1 = reader.ReadUInt32(),
            Fade2 = reader.ReadUInt32()
        };
    }

    public static TR2RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
    {
        return new TR2RoomStaticMesh
        {
            X = reader.ReadUInt32(),
            Y = reader.ReadUInt32(),
            Z = reader.ReadUInt32(),
            Rotation = reader.ReadUInt16(),
            Intensity1 = reader.ReadUInt16(),
            Intensity2 = reader.ReadUInt16(),
            MeshID = reader.ReadUInt16()
        };
    }

    public static TRAnimCommand ReadAnimCommand(BinaryReader reader)
    {
        return new TRAnimCommand()
        {
            Value = reader.ReadInt16()
        };
    }

    public static TRMeshTreeNode ReadMeshTreeNode(BinaryReader reader)
    {
        return new TRMeshTreeNode()
        {
            Flags = reader.ReadUInt32(),
            OffsetX = reader.ReadInt32(),
            OffsetY = reader.ReadInt32(),
            OffsetZ = reader.ReadInt32()
        };
    }

    public static TRVertex ReadVertex(BinaryReader reader)
    {
        return new TRVertex
        {
            X = reader.ReadInt16(),
            Y = reader.ReadInt16(),
            Z = reader.ReadInt16()
        };
    }

    public static TRStaticMesh ReadStaticMesh(BinaryReader reader)
    {
        return new TRStaticMesh()
        {
            ID = reader.ReadUInt32(),
            Mesh = reader.ReadUInt16(),
            VisibilityBox = new TRBoundingBox
            {
                MinX = reader.ReadInt16(),
                MaxX = reader.ReadInt16(),
                MinY = reader.ReadInt16(),
                MaxY = reader.ReadInt16(),
                MinZ = reader.ReadInt16(),
                MaxZ = reader.ReadInt16()
            },
            CollisionBox = new TRBoundingBox
            {
                MinX = reader.ReadInt16(),
                MaxX = reader.ReadInt16(),
                MinY = reader.ReadInt16(),
                MaxY = reader.ReadInt16(),
                MinZ = reader.ReadInt16(),
                MaxZ = reader.ReadInt16()
            },
            Flags = reader.ReadUInt16()
        };
    }

    public static TRObjectTexture ReadObjectTexture(BinaryReader reader)
    {
        return new TRObjectTexture()
        {
            Attribute = reader.ReadUInt16(),
            AtlasAndFlag = reader.ReadUInt16(),
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
            }
        };
    }

    public static TRSpriteTexture ReadSpriteTexture(BinaryReader reader)
    {
        return new TRSpriteTexture()
        {
            Atlas = reader.ReadUInt16(),
            X = reader.ReadByte(),
            Y = reader.ReadByte(),
            Width = reader.ReadUInt16(),
            Height = reader.ReadUInt16(),
            LeftSide = reader.ReadInt16(),
            TopSide = reader.ReadInt16(),
            RightSide = reader.ReadInt16(),
            BottomSide = reader.ReadInt16()
        };
    }

    public static TRSpriteSequence ReadSpriteSequence(BinaryReader reader)
    {
        return new TRSpriteSequence()
        {
            SpriteID = reader.ReadInt32(),
            NegativeLength = reader.ReadInt16(),
            Offset = reader.ReadInt16()
        };
    }

    public static TRCamera ReadCamera(BinaryReader reader)
    {
        return new TRCamera()
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Room = reader.ReadInt16(),
            Flag = reader.ReadUInt16()
        };
    }

    public static TRSoundSource ReadSoundSource(BinaryReader reader)
    {
        return new TRSoundSource()
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            SoundID = reader.ReadUInt16(),
            Flags = reader.ReadUInt16()
        };
    }

    public static TR2Box ReadBox(BinaryReader reader)
    {
        return new TR2Box()
        {
            ZMin = reader.ReadByte(),
            ZMax = reader.ReadByte(),
            XMin = reader.ReadByte(),
            XMax = reader.ReadByte(),
            TrueFloor = reader.ReadInt16(),
            OverlapIndex = reader.ReadInt16()
        };
    }

    public static TRAnimatedTexture ReadAnimatedTexture(BinaryReader reader)
    {
        // See https://opentomb.github.io/TRosettaStone3/trosettastone.html#_animated_textures_2
        int numTextures = reader.ReadUInt16() + 1;
        List<ushort> textures = new();
        for (int i = 0; i < numTextures; i++)
        {
            textures.Add(reader.ReadUInt16());
        }

        return new TRAnimatedTexture
        {
            Textures = textures
        };
    }

    public static TRCinematicFrame ReadCinematicFrame(BinaryReader reader)
    {
        return new TRCinematicFrame()
        {
            TargetX = reader.ReadInt16(),
            TargetY = reader.ReadInt16(),
            TargetZ = reader.ReadInt16(),
            PosZ = reader.ReadInt16(),
            PosY = reader.ReadInt16(),
            PosX = reader.ReadInt16(),
            FOV = reader.ReadInt16(),
            Roll = reader.ReadInt16()
        };
    }
}
