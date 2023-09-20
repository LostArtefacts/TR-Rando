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
        chunk.CompressedChunk = ReadBytes((int)chunk.CompressedSize);

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

    public List<TRColour> ReadColours(long numColours)
    {
        List<TRColour> colours = new((int)numColours);
        for (long i = 0; i < numColours; i++)
        {
            colours.Add(new()
            {
                Red = ReadByte(),
                Green = ReadByte(),
                Blue = ReadByte()
            });
        }
        return colours;
    }

    public List<TRColour4> ReadColour4s(long numColours)
    {
        List<TRColour4> colours = new((int)numColours);
        for (long i = 0; i < numColours; i++)
        {
            colours.Add(new()
            {
                Red = ReadByte(),
                Green = ReadByte(),
                Blue = ReadByte(),
                Alpha = ReadByte()
            });
        }
        return colours;
    }

    public List<TR1Entity> ReadTR1Entities(long numEntities)
    {
        List<TR1Entity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR1Entity());
        }
        return entities;
    }

    public TR1Entity ReadTR1Entity()
    {
        return new()
        {
            TypeID = ReadInt16(),
            Room = ReadInt16(),
            X = ReadInt32(),
            Y = ReadInt32(),
            Z = ReadInt32(),
            Angle = ReadInt16(),
            Intensity = ReadInt16(),
            Flags = ReadUInt16()
        };
    }

    public List<TR2Entity> ReadTR2Entities(long numEntities)
    {
        List<TR2Entity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR2Entity());
        }
        return entities;
    }

    public TR2Entity ReadTR2Entity()
    {
        return new()
        {
            TypeID = ReadInt16(),
            Room = ReadInt16(),
            X = ReadInt32(),
            Y = ReadInt32(),
            Z = ReadInt32(),
            Angle = ReadInt16(),
            Intensity1 = ReadInt16(),
            Intensity2 = ReadInt16(),
            Flags = ReadUInt16()
        };
    }

    public List<TR4Entity> ReadTR4Entities(long numEntities)
    {
        List<TR4Entity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR4Entity());
        }
        return entities;
    }

    public TR4Entity ReadTR4Entity()
    {
        return new()
        {
            TypeID = ReadInt16(),
            Room = ReadInt16(),
            X = ReadInt32(),
            Y = ReadInt32(),
            Z = ReadInt32(),
            Angle = ReadInt16(),
            Intensity = ReadInt16(),
            OCB = ReadInt16(),
            Flags = ReadUInt16()
        };
    }
}
