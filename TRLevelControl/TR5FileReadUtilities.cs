using System.Diagnostics;

using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR5FileReadUtilities
{
    public static readonly string TEXMarker = "TEX\0";

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
}
