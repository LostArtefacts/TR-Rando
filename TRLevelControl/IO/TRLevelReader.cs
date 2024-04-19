using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelReader : BinaryReader
{
    private readonly ITRLevelObserver _observer;

    public TRLevelReader(Stream stream, ITRLevelObserver observer = null)
        : base(stream)
    {
        _observer = observer;
    }

    public TRLevelReader Inflate()
    {
        uint expectedLength = ReadUInt32();
        uint compressedLength = ReadUInt32();

        byte[] data = new byte[compressedLength];
        for (uint i = 0; i < compressedLength; i++)
        {
            data[i] = ReadByte();
        }

        MemoryStream inflatedStream;

        inflatedStream = new();
        using MemoryStream ms = new(data);
        using InflaterInputStream inflater = new(ms);

        inflater.CopyTo(inflatedStream);

        if (inflatedStream.Length != expectedLength)
        {
            throw new InvalidDataException($"Inflated stream length mismatch: got {inflatedStream.Length}, expected {expectedLength}");
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
            images.Add(ReadImage32());
        }
        return images;
    }

    public TRTexImage32 ReadImage32()
    {
        return new()
        {
            Pixels = ReadUInt32s(TRConsts.TPageSize)
        };
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
            TypeID = (TR1Type)ReadInt16(),
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
            TypeID = (TR2Type)ReadInt16(),
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

    public List<TR3Entity> ReadTR3Entities(long numEntities)
    {
        List<TR3Entity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR3Entity());
        }
        return entities;
    }

    public TR3Entity ReadTR3Entity()
    {
        return new()
        {
            TypeID = (TR3Type)ReadInt16(),
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
            TypeID = (TR4Type)ReadInt16(),
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

    public List<TR4AIEntity> ReadTR4AIEntities(long numEntities)
    {
        List<TR4AIEntity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR4AIEntity());
        }
        return entities;
    }

    public TR4AIEntity ReadTR4AIEntity()
    {
        return new()
        {
            TypeID = (TR4Type)ReadInt16(),
            Room = ReadInt16(),
            X = ReadInt32(),
            Y = ReadInt32(),
            Z = ReadInt32(),
            OCB = ReadInt16(),
            Flags = ReadUInt16(),
            Angle = ReadInt16(),
            Box = ReadInt16()
        };
    }

    public List<TR5Entity> ReadTR5Entities(long numEntities)
    {
        List<TR5Entity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR5Entity());
        }
        return entities;
    }

    public TR5Entity ReadTR5Entity()
    {
        return new()
        {
            TypeID = (TR5Type)ReadInt16(),
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

    public List<TR5AIEntity> ReadTR5AIEntities(long numEntities)
    {
        List<TR5AIEntity> entities = new();
        for (int i = 0; i < numEntities; i++)
        {
            entities.Add(ReadTR5AIEntity());
        }
        return entities;
    }

    public TR5AIEntity ReadTR5AIEntity()
    {
        return new()
        {
            TypeID = (TR5Type)ReadInt16(),
            Room = ReadInt16(),
            X = ReadInt32(),
            Y = ReadInt32(),
            Z = ReadInt32(),
            OCB = ReadInt16(),
            Flags = ReadUInt16(),
            Angle = ReadInt16(),
            Box = ReadInt16()
        };
    }
}
