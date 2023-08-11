namespace TRLevelControl.Model;

public class TR4Chunk
{
    public uint UncompressedSize { get; set; }
    public uint CompressedSize { get; set; }
    public byte[] CompressedChunk { get; set; }
}
