using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR2FileReadUtilities
{
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
