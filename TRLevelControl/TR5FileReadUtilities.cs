using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR5FileReadUtilities
{
    public static readonly string TEXMarker = "TEX\0";

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
}
