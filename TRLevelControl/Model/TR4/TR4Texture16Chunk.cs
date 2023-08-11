using TRLevelControl.Compression;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4Texture16Chunk : TR4Chunk, ISerializableCompact
{
    public List<TRTexImage16> Rooms { get; set; }
    public List<TRTexImage16> Objects { get; set; }
    public List<TRTexImage16> Bump { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            foreach (TRTexImage16 tex in Rooms) { writer.Write(tex.Serialize()); }
            foreach (TRTexImage16 tex in Objects) { writer.Write(tex.Serialize()); }
            foreach (TRTexImage16 tex in Bump) { writer.Write(tex.Serialize()); }
        }

        byte[] uncompressed = stream.ToArray();
        UncompressedSize = (uint)uncompressed.Length;

        byte[] compressed = TRZlib.Compress(uncompressed);
        CompressedSize = (uint)compressed.Length;

        return compressed;
    }
}
