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

    public TRLevelReader Inflate(TRChunkType chunkType)
    {
        long position = BaseStream.Position;
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

        _observer?.OnChunkRead(position, BaseStream.Position, chunkType, inflatedStream.ToArray());

        if (inflatedStream.Length != expectedLength)
        {
            throw new InvalidDataException($"Inflated stream length mismatch: got {inflatedStream.Length}, expected {expectedLength}");
        }

        inflatedStream.Position = 0;
        return new(inflatedStream);
    }

    public byte[] ReadUInt8s(long numData)
    {
        byte[] data = new byte[numData];
        for (int i = 0; i < numData; i++)
        {
            data[i] = ReadByte();
        }

        return data;
    }

    public short[] ReadInt16s(long numData)
    {
        short[] data = new short[numData];
        for (int i = 0; i < numData; i++)
        {
            data[i] = ReadInt16();
        }

        return data;
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

    public List<TRMeshFace> ReadMeshFaces(long numFaces, TRFaceType type, TRGameVersion version)
    {
        List<TRMeshFace> faces = new();
        for (int i = 0; i < numFaces; i++)
        {
            faces.Add(ReadMeshFace(type, version));
        }
        return faces;
    }

    public TRMeshFace ReadMeshFace(TRFaceType type, TRGameVersion version)
    {
        TRMeshFace face = new()
        {
            Type = type,
            Vertices = new(ReadUInt16s((int)type)),
        };

        ReadFaceTexture(face, version);
        if (version >= TRGameVersion.TR4)
        {
            face.Effects = ReadUInt16();
        }

        return face;
    }

    private void ReadFaceTexture(TRFace face, TRGameVersion version)
    {
        ushort texture = ReadUInt16();
        if (version < TRGameVersion.TR3)
        {
            // No extra flags
            face.Texture = texture;
        }
        else
        {
            face.Texture = (ushort)(texture & (version == TRGameVersion.TR5 ? 0x3FFF : 0x7FFF));
            face.DoubleSided = (texture & 0x8000) > 0;
            if (version == TRGameVersion.TR5)
            {
                face.UnknownFlag = (texture & 0x4000) > 0;
            }
        }
    }

    public FixedFloat32 ReadFixed32()
    {
        return new()
        {
            Whole = ReadInt16(),
            Fraction = ReadUInt16()
        };
    }

    public TRBoundingBox ReadBoundingBox()
    {
        return new()
        {
            MinX = ReadInt16(),
            MaxX = ReadInt16(),
            MinY = ReadInt16(),
            MaxY = ReadInt16(),
            MinZ = ReadInt16(),
            MaxZ = ReadInt16()
        };
    }

    public List<TRSpriteTexture> ReadSpriteTextures(long numTextures, TRGameVersion version)
    {
        List<TRSpriteTexture> textures = new();
        for (int i = 0; i < numTextures; i++)
        {
            textures.Add(ReadSpriteTexture(version));
        }
        return textures;
    }

    public TRSpriteTexture ReadSpriteTexture(TRGameVersion version)
    {
        TRSpriteTexture sprite = new()
        {
            Atlas = ReadUInt16(),
        };

        byte x = ReadByte();
        byte y = ReadByte();
        ushort width = ReadUInt16();
        ushort height = ReadUInt16();
        short left = ReadInt16();
        short top = ReadInt16();
        short right = ReadInt16();
        short bottom = ReadInt16();

        if (version < TRGameVersion.TR4)
        {
            sprite.X = x;
            sprite.Y = y;
            sprite.Width = (ushort)((width + 1) / TRConsts.TPageWidth);
            sprite.Height = (ushort)((height + 1) / TRConsts.TPageHeight);
            sprite.Alignment = new()
            {
                Left = left,
                Top = top,
                Right = right,
                Bottom = bottom
            };
        }
        else
        {
            sprite.X = (byte)left;
            sprite.Y = (byte)top;
            sprite.Width = (ushort)(width / TRConsts.TPageWidth + 1);
            sprite.Height = (ushort)(height / TRConsts.TPageHeight + 1);
            sprite.Alignment = new()
            {
                Left = x,
                Top = y,
                Right = right,
                Bottom = bottom
            };
        }

        return sprite;
    }
}
