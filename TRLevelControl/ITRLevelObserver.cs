using TRLevelControl.Model;

namespace TRLevelControl;

public interface ITRLevelObserver
{
    void OnChunkRead(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    void OnChunkWritten(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    void OnMeshPaddingRead(uint meshPointer, List<byte> values);
    List<byte> GetMeshPadding(uint meshPointer);
}
