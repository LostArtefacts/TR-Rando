using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using TRLevelControl.Model;
using TRLevelControl.Model.TRX;

namespace TRLevelControl.IO;

public static class TRXInjector
{
    private const uint _magic = 'T' | 'R' << 8 | 'X' << 16 | 'J' << 24;
    private const uint _version = 4;

    public static TRXInjectionData Read(TRLevelReader reader)
    {
        try
        {
            if (reader.ReadUInt32() != _magic)
            {
                return null;
            }
        }
        catch (EndOfStreamException)
        {
            return null;
        }

        // Skip version and config option value
        reader.BaseStream.Position += 2 * sizeof(uint);

        var zipReader = reader.Inflate(TRChunkType.LevelData);

        // Ignore tests
        zipReader.BaseStream.Position += sizeof(int);
        var testLength = zipReader.ReadInt32();
        zipReader.BaseStream.Position += testLength;

        var chunkCount = zipReader.ReadInt32();
        var data = new TRXInjectionData();
        for (int i = 0; i < chunkCount; i++)
        {
            var chunk = TRXChunk.Read(zipReader);
            using var chunkMS = new MemoryStream(chunk.Data);
            using var chunkReader = new TRLevelReader(chunkMS);
            ReadChunk(data, chunk, chunkReader);
        }

        return data;
    }

    private static void ReadChunk(TRXInjectionData data, TRXChunk chunk, TRLevelReader reader)
    {
        for (int i = 0; i < chunk.BlockCount; i++)
        {
            var blockType = (TRXBlockType)reader.ReadInt32();
            var blockCount = reader.ReadInt32();
            reader.BaseStream.Position += sizeof(int); // Skip total length

            for (int j = 0; j < blockCount; j++)
            {
                switch (blockType)
                {
                    case TRXBlockType.SampleInfos:
                        data.SFX.Add(TRSFXData.Read(reader));
                        break;
                }
            }
        }
    }

    public static void Write(TRXInjectionData data, TRLevelWriter outWriter)
    {
        var chunks = Chunkify(data);
        if (chunks == null || chunks.Count == 0)
        {
            return;
        }

        using var injStream = new MemoryStream();
        using var injWriter = new TRLevelWriter(injStream);

        injWriter.Write(0); // Number of tests
        injWriter.Write(0); // Total length of tests
        injWriter.Write(chunks.Count);
        chunks.ForEach(c => c.Serialize(injWriter));

        var inflatedData = injStream.ToArray();
        using var outStream = new MemoryStream();
        using var deflater = new DeflaterOutputStream(outStream);
        using var inStream = new MemoryStream(inflatedData);

        inStream.CopyTo(deflater);
        deflater.Finish();

        var deflatedData = outStream.ToArray();

        outWriter.Write(_magic);
        outWriter.Write(_version);
        outWriter.Write(0); // Normally a value to link to a config option

        outWriter.Write(inflatedData.Length);
        outWriter.Write(deflatedData.Length);
        outWriter.Write(deflatedData);
    }

    private static List<TRXChunk> Chunkify(TRXInjectionData data)
    {
        if (data == null)
        {
            return null;
        }

        var chunks = new List<TRXChunk>
        {
            CreateChunk(TRXChunkType.SFXData, data, WriteSFXData),
        };

        chunks.RemoveAll(c => c.BlockCount == 0);
        return chunks;
    }

    private static TRXChunk CreateChunk(TRXChunkType type,
        TRXInjectionData data, Func<TRXInjectionData, TRLevelWriter, int> process)
    {
        using var stream = new MemoryStream();
        using var writer = new TRLevelWriter(stream);
        int blockCount = process(data, writer);

        return new()
        {
            Type = type,
            BlockCount = blockCount,
            Data = stream.ToArray(),
        };
    }

    private static int WriteSFXData(TRXInjectionData data, TRLevelWriter writer)
    {
        return WriteBlock(TRXBlockType.SampleInfos, data.SFX.Count, writer,
            s => data.SFX.ForEach(f => f.Write(s)));
    }

    private static int WriteBlock(TRXBlockType type, int elementCount,
        TRLevelWriter writer, Action<TRLevelWriter> subCallback)
    {
        if (elementCount == 0)
        {
            return 0;
        }

        using var stream = new MemoryStream();
        using var subWriter = new TRLevelWriter(stream);
        subCallback(subWriter);
        subWriter.Flush();

        var data = stream.ToArray();
        writer.Write((int)type);
        writer.Write(elementCount);
        writer.Write(data.Length);
        writer.Write(data);

        return 1;
    }

    private class TRXChunk
    {
        public TRXChunkType Type { get; set; }
        public int BlockCount { get; set; }
        public byte[] Data { get; set; }

        public void Serialize(TRLevelWriter writer)
        {
            writer.Write((int)Type);
            writer.Write(BlockCount);
            writer.Write(Data.Length);
            writer.Write(Data);
        }

        public static TRXChunk Read(TRLevelReader reader)
        {
            var chunk = new TRXChunk
            {
                Type = (TRXChunkType)reader.ReadInt32(),
                BlockCount = reader.ReadInt32(),
            };
            int blockLength = reader.ReadInt32();
            chunk.Data = reader.ReadBytes(blockLength);
            return chunk;
        }
    }

    private enum TRXChunkType
    {
        SFXData = 5,
    }

    private enum TRXBlockType
    {
        SampleInfos = 14,
    }
}
