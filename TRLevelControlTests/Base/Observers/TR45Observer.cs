using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class TR45Observer : ObserverBase
{
    private readonly Dictionary<TRChunkType, ZipWrapper> _inflatedReads = new();
    private readonly Dictionary<TRChunkType, ZipWrapper> _inflatedWrites = new();

    public override void TestOutput(byte[] input, byte[] output)
    {
        CollectionAssert.AreEquivalent(_inflatedReads.Keys, _inflatedWrites.Keys);

        foreach (TRChunkType type in _inflatedReads.Keys)
        {
            CollectionAssert.AreEqual(_inflatedReads[type].Data, _inflatedWrites[type].Data);
        }
    }

    public override void OnChunkRead(long startPosition, long endPosition, TRChunkType chunkType, byte[] data)
    {
        _inflatedReads[chunkType] = new()
        {
            StreamStart = startPosition,
            StreamEnd = endPosition,
            Data = data
        };
    }

    public override void OnChunkWritten(long startPosition, long endPosition, TRChunkType chunkType, byte[] data)
    {
        _inflatedWrites[chunkType] = new()
        {
            StreamStart = startPosition,
            StreamEnd = endPosition,
            Data = data
        };
    }

    class ZipWrapper
    {
        public long StreamStart { get; set; }
        public long StreamEnd { get; set; }
        public byte[] Data { get; set; }
    }
}
