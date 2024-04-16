using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimatedTexture : ISerializableCompact
{
    public List<ushort> Textures { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write((ushort)(Textures.Count - 1));
        foreach (ushort texture in Textures)
        {
            writer.Write(texture);
        }

        return stream.ToArray();
    }
}
