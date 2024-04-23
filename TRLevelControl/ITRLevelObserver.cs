using TRLevelControl.Model;

namespace TRLevelControl;

public interface ITRLevelObserver
{
    void OnChunkRead(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    void OnChunkWritten(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    void OnMeshPaddingRead(uint meshPointer, List<byte> values);
    List<byte> GetMeshPadding(uint meshPointer);
    void OnBadDispatchLinkRead(int dispatchIndex, short animLink, short frameLink);
    Tuple<short, short> GetDispatchLink(int dispatchIndex);
    void OnBadAnimLinkRead(int animIndex, ushort animLink, ushort frameLink);
    Tuple<ushort, ushort> GetAnimLink(int animIndex);
    void OnBadAnimCommandRead(int animIndex, ushort numAnimCommands);
    ushort? GetNumAnimCommands(int animIndex);
    void OnAnimCommandPaddingRead(short padding);
    short? GetAnimCommandPadding();
    void OnUnusedStateChangeRead(Tuple<ushort, ushort> padding);
    Tuple<ushort, ushort> GetUnusedStateChange();
    void OnSampleIndicesRead(uint[] sampleIndices);
    IEnumerable<uint> GetSampleIndices();
}
