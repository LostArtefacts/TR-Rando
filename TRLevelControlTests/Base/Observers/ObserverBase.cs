using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    public virtual void OnMeshPaddingRead(uint meshPointer, List<byte> values)
    { }

    public virtual List<byte> GetMeshPadding(uint meshPointer)
        => null;
}
