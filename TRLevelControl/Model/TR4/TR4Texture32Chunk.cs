using TRLevelControl.Compression;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4Texture32Chunk : TR4Chunk, ISerializableCompact
{
    public List<TR4TexImage32> Rooms { get; set; }
    public List<TR4TexImage32> Objects { get; set; }
    public List<TR4TexImage32> Bump { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            foreach (TR4TexImage32 tex in Rooms) { writer.Write(tex.Serialize()); }
            foreach (TR4TexImage32 tex in Objects) { writer.Write(tex.Serialize()); }
            foreach (TR4TexImage32 tex in Bump) { writer.Write(tex.Serialize()); }
        }

        byte[] uncompressed = stream.ToArray();
        UncompressedSize = (uint)uncompressed.Length;

        byte[] compressed = TRZlib.Compress(uncompressed);
        CompressedSize = (uint)compressed.Length;

        return compressed;
    }
}
