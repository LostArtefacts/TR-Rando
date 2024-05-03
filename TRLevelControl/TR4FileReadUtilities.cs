using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR4FileReadUtilities
{
    public static readonly string TEXMarker = "TEX";

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
