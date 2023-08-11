using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelReader : BinaryReader
{
    public TRLevelReader(Stream stream)
        : base(stream) { }

    public TRLevelReader Inflate(TR4Chunk chunk)
    {
        chunk.UncompressedSize = ReadUInt32();
        chunk.CompressedSize = ReadUInt32();

        chunk.CompressedChunk = new byte[chunk.CompressedSize];
        for (uint i = 0; i < chunk.CompressedSize; i++)
        {
            chunk.CompressedChunk[i] = ReadByte();
        }

        MemoryStream inflatedStream = new();
        using MemoryStream ms = new(chunk.CompressedChunk);
        using InflaterInputStream inflater = new(ms);
        inflater.CopyTo(inflatedStream);

        if (inflatedStream.Length != chunk.UncompressedSize)
        {
            throw new InvalidDataException(
                $"Inflated stream length mismatch: got {inflatedStream.Length}, expected {chunk.UncompressedSize}");
        }

        inflatedStream.Position = 0;
        return new(inflatedStream);
    }

    public ushort[] ReadUInt16s(long numData)
    {
        ushort[] data = new ushort[numData];
        for (int i = 0; i < numData; i++)
        {
            data[i] = ReadUInt16();
        }

        return data;
    }

    public uint[] ReadUInt32s(long numData)
    {
        uint[] data = new uint[numData];
        for (int i = 0; i < numData; i++)
        {
            data[i] = ReadUInt32();
        }

        return data;
    }

    public List<TRTexImage8> ReadImage8s(long numImages)
    {
        List<TRTexImage8> images = new((int)numImages);
        for (long i = 0; i < numImages; i++)
        {
            images.Add(new()
            {
                Pixels = ReadBytes(TRConsts.TPageSize)
            });
        }
        return images;
    }

    public List<TRTexImage16> ReadImage16s(long numImages)
    {
        List<TRTexImage16> images = new((int)numImages);
        for (long i = 0; i < numImages; i++)
        {
            images.Add(new()
            {
                Pixels = ReadUInt16s(TRConsts.TPageSize)
            });
        }
        return images;
    }

    public List<TRTexImage32> ReadImage32s(long numImages)
    {
        List<TRTexImage32> images = new((int)numImages);
        for (long i = 0; i < numImages; i++)
        {
            images.Add(new()
            {
                Pixels = ReadUInt32s(TRConsts.TPageSize)
            });
        }
        return images;
    }
}
