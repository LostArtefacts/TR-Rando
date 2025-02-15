using TRLevelControl.Model;

namespace TRLevelControl;

public interface ITRLevelObserver
{
    void OnChunkRead(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    void OnChunkWritten(long startPosition, long endPosition, TRChunkType chunkType, byte[] data);
    bool UseTR5RawRooms { get; }
    bool UseOriginalFloorData { get; }
    void OnFloorDataRead(ushort[] data);
    ushort[] GetFloorData();
    void OnRawTR5RoomsRead(List<byte> data);
    List<byte> GetTR5Rooms();
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
    void OnEmptyAnimFramesRead(int animIndex, byte frameSize);
    byte? GetEmptyAnimFrameSize(int animIndex);
    void OnFramePaddingRead(int animIndex, int frameIndex, List<short> values);
    List<short> GetFramePadding(int animIndex, int frameIndex);
    void OnBadOverlapRead(ushort value);
    ushort? GetBadOverlap();
    void OnOrignalUVRead(int index, Tuple<uint, uint> uv);
    Tuple<uint, uint> GetOrignalUV(int index);
    void OnFlybyIndexRead(byte flybySequence, byte cameraIndex);
    List<byte> GetFlybyIndices(byte flybySequence);
    void OnSampleIndicesRead(uint[] sampleIndices);
    IEnumerable<uint> GetSampleIndices();
}
