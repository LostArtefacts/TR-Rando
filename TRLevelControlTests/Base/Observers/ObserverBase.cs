using TRLevelControl;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class ObserverBase : ITRLevelObserver
{
    public virtual void TestOutput(byte[] input, byte[] output)
    {
        CollectionAssert.AreEqual(input, output);
    }

    public virtual void OnChunkRead(long startPosition, long endPosition, TRChunkType chunkType, byte[] data)
    { }

    public virtual void OnChunkWritten(long startPosition, long endPosition, TRChunkType chunkType, byte[] data)
    { }

    public virtual bool UseOriginalFloorData => true;
    public virtual bool UseTR5RawRooms => false;

    public virtual void OnFloorDataRead(ushort[] data)
    { }

    public virtual ushort[] GetFloorData() => null;

    public virtual void OnRawTR5RoomsRead(List<byte> data)
    { }

    public virtual List<byte> GetTR5Rooms()
        => null;

    public virtual void OnMeshPaddingRead(uint meshPointer, List<byte> values)
    { }

    public virtual List<byte> GetMeshPadding(uint meshPointer)
        => null;

    public virtual void OnBadDispatchLinkRead(int dispatchIndex, short animLink, short frameLink)
    { }

    public virtual Tuple<short, short> GetDispatchLink(int dispatchIndex)
        => null;

    public virtual void OnBadAnimLinkRead(int animIndex, ushort animLink, ushort frameLink)
    { }

    public virtual Tuple<ushort, ushort> GetAnimLink(int animIndex)
        => null;

    public virtual void OnBadAnimCommandRead(int animIndex, ushort numAnimCommands)
    { }

    public virtual ushort? GetNumAnimCommands(int animIndex)
        => null;

    public virtual void OnAnimCommandPaddingRead(short padding)
    { }

    public virtual short? GetAnimCommandPadding()
        => null;

    public virtual void OnUnusedStateChangeRead(Tuple<ushort, ushort> padding)
    { }

    public virtual Tuple<ushort, ushort> GetUnusedStateChange()
        => null;

    public virtual void OnEmptyAnimFramesRead(int animIndex, byte frameSize)
    { }

    public virtual byte? GetEmptyAnimFrameSize(int animIndex)
        => null;

    public virtual void OnFramePaddingRead(int animIndex, int frameIndex, List<short> values)
    { }

    public virtual List<short> GetFramePadding(int animIndex, int frameIndex)
        => null;

    public virtual void OnBadOverlapRead(ushort value)
    { }

    public virtual ushort? GetBadOverlap()
        => null;

    public virtual void OnOriginalUVRead(int index, uint[] uv)
    { }

    public virtual uint[] GetOriginalUV(int index)
        => null;

    public virtual void OnSampleIndicesRead(uint[] sampleIndices)
    { }

    public virtual IEnumerable<uint> GetSampleIndices()
        => null;

    public virtual void OnFlybyIndexRead(byte flybySequence, byte cameraIndex)
    { }

    public virtual List<byte> GetFlybyIndices(byte flybySequence)
        => null;
}
