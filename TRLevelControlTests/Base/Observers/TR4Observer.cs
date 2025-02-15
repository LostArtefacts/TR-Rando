using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests;

public class TR4Observer : TR3Observer
{
    private readonly Dictionary<TRChunkType, ZipWrapper> _inflatedReads = new();
    private readonly Dictionary<TRChunkType, ZipWrapper> _inflatedWrites = new();

    private readonly Dictionary<uint, List<byte>> _meshPadding = new();
    private readonly Dictionary<int, ushort> _badAnimCmdCounts = new();
    private readonly Dictionary<int, byte> _emptyAnimFrameSizes = new();
    private readonly Dictionary<int, Tuple<uint, uint>> _originalUV = new();
    private readonly Dictionary<byte, List<byte>> _flybyIndices = new();

    private uint[] _sampleIndices;
    protected readonly bool _remastered;
    private ushort[] _floorData;

    public TR4Observer(bool remastered)
    {
        _remastered = remastered;
    }

    public override bool UseOriginalFloorData => _remastered;

    public override void OnFloorDataRead(ushort[] data)
    {
        _floorData = data;
    }

    public override ushort[] GetFloorData() => _floorData;

    public override void TestOutput(byte[] input, byte[] output)
    {
        CollectionAssert.AreEquivalent(_inflatedReads.Keys, _inflatedWrites.Keys);

        foreach (TRChunkType type in _inflatedReads.Keys)
        {
            CollectionAssert.AreEqual(_inflatedReads[type].Data, _inflatedWrites[type].Data);
        }

        // At this stage, everything zipped matches. We want to check for unzipped matches, so do
        // so by stripping out everything that has been zipped from both streams.
        List<byte> oldData = new(input);
        List<byte> newData = new(output);

        List<ZipWrapper> unzips = new(_inflatedReads.Values);
        unzips.Sort((z1, z2) => z2.StreamEnd.CompareTo(z1.StreamEnd));
        foreach (ZipWrapper zip in unzips)
        {
            oldData.RemoveRange((int)zip.StreamStart, (int)(zip.StreamEnd - zip.StreamStart));
        }

        unzips = new(_inflatedWrites.Values);
        unzips.Sort((z1, z2) => z2.StreamEnd.CompareTo(z1.StreamEnd));
        foreach (ZipWrapper zip in unzips)
        {
            newData.RemoveRange((int)zip.StreamStart, (int)(zip.StreamEnd - zip.StreamStart));
        }

        CollectionAssert.AreEqual(oldData, newData);
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

    public override void OnMeshPaddingRead(uint meshPointer, List<byte> values)
    {
        _meshPadding[meshPointer] = values;
    }

    public override List<byte> GetMeshPadding(uint meshPointer)
    {
        return _meshPadding.ContainsKey(meshPointer) ? _meshPadding[meshPointer] : null;
    }

    public override void OnBadAnimCommandRead(int animIndex, ushort numAnimCommands)
    {
        _badAnimCmdCounts[animIndex] = numAnimCommands;
    }

    public override ushort? GetNumAnimCommands(int animIndex)
    {
        return _badAnimCmdCounts.ContainsKey(animIndex) ? _badAnimCmdCounts[animIndex] : null;
    }

    public override void OnEmptyAnimFramesRead(int animIndex, byte frameSize)
    {
        _emptyAnimFrameSizes[animIndex] = frameSize;
    }

    public override byte? GetEmptyAnimFrameSize(int animIndex)
    {
        return _emptyAnimFrameSizes.ContainsKey(animIndex) ? _emptyAnimFrameSizes[animIndex] : null;
    }

    public override void OnFlybyIndexRead(byte flybySequence, byte cameraIndex)
    {
        if (!_flybyIndices.ContainsKey(flybySequence))
        {
            _flybyIndices[flybySequence] = new();
        }
        _flybyIndices[flybySequence].Add(cameraIndex);
    }

    public override List<byte> GetFlybyIndices(byte flybySequence)
    {
        return _flybyIndices.ContainsKey(flybySequence) ? _flybyIndices[flybySequence] : null;
    }

    public override void OnOrignalUVRead(int index, Tuple<uint, uint> uv)
    {
        _originalUV[index] = uv;
    }

    public override Tuple<uint, uint> GetOrignalUV(int index)
    {
        return _originalUV.ContainsKey(index) ? _originalUV[index] : null;
    }

    public override void OnSampleIndicesRead(uint[] sampleIndices)
    {
        _sampleIndices = sampleIndices;
    }

    public override IEnumerable<uint> GetSampleIndices()
    {
        return _sampleIndices;
    }

    class ZipWrapper
    {
        public long StreamStart { get; set; }
        public long StreamEnd { get; set; }
        public byte[] Data { get; set; }
    }
}
