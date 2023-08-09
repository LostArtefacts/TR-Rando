using TRLevelControl.Compression;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4Texture16Chunk : ISerializableCompact
{
    public uint UncompressedSize { get; set; }

    public uint CompressedSize { get; set; }

    public TRTexImage16[] Textiles { get; set; }

    //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
    public byte[] CompressedChunk { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            foreach (TRTexImage16 tex in Textiles) { writer.Write(tex.Serialize()); }
        }

        byte[] uncompressed = stream.ToArray();
        UncompressedSize = (uint)uncompressed.Length;

        byte[] compressed = TRZlib.Compress(uncompressed);
        CompressedSize = (uint)compressed.Length;

        return compressed;
    }
}
