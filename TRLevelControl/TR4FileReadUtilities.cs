using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR4FileReadUtilities
{
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
}
