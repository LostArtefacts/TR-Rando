using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimatedTexture : ISerializableCompact
{
    //https://opentomb.github.io/TRosettaStone3/trosettastone.html#_animated_textures_2
    public ushort NumTextures => (ushort)(Textures.Length - 1);
    public ushort[] Textures { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(NumTextures);
        foreach (ushort texture in Textures)
        {
            writer.Write(texture);
        }

        return stream.ToArray();
    }
}
