using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelWriter : BinaryWriter
{
    private readonly ITRLevelObserver _observer;

    public TRLevelWriter(ITRLevelObserver observer = null)
        : this(new MemoryStream(), observer) { }

    public TRLevelWriter(Stream input, ITRLevelObserver observer = null)
        : base(input)
    {
        _observer = observer;
    }

    public void Deflate(TRLevelWriter inflatedWriter, TRChunkType chunkType)
    {
        byte[] data = (inflatedWriter.BaseStream as MemoryStream).ToArray();

        using MemoryStream outStream = new();
        using DeflaterOutputStream deflater = new(outStream);
        using MemoryStream inStream = new(data);

        inStream.CopyTo(deflater);
        deflater.Finish();

        byte[] zippedData = outStream.ToArray();
        long startPosition = BaseStream.Position;
        Write((uint)data.Length);
        Write((uint)zippedData.Length);
        Write(zippedData);

        _observer?.OnChunkWritten(startPosition, BaseStream.Position, chunkType, data);
    }

    public void Write(IEnumerable<byte> data)
    {
        foreach (byte value in data)
        {
            Write(value);
        }
    }

    public void Write(IEnumerable<short> data)
    {
        foreach (short value in data)
        {
            Write(value);
        }
    }

    public void Write(IEnumerable<ushort> data)
    {
        foreach (ushort value in data)
        {
            Write(value);
        }
    }

    public void Write(IEnumerable<uint> data)
    {
        foreach (uint value in data)
        {
            Write(value);
        }
    }

    public void Write(IEnumerable<TRTexImage8> images)
    {
        foreach (TRTexImage8 image in images)
        {
            Write(image.Pixels);
        }
    }

    public void Write(IEnumerable<TRTexImage16> images)
    {
        foreach (TRTexImage16 image in images)
        {
            Write(image.Pixels);
        }
    }

    public void Write(IEnumerable<TRTexImage32> images)
    {
        foreach (TRTexImage32 image in images)
        {
            Write(image);
        }
    }

    public void Write(TRTexImage32 image)
    {
        Write(image.Pixels);
    }

    public void Write(IEnumerable<TRColour> colours)
    {
        foreach (TRColour colour in colours)
        {
            Write(colour);
        }
    }

    public void Write(TRColour colour)
    {
        Write(colour.Red);
        Write(colour.Green);
        Write(colour.Blue);
    }

    public void Write(IEnumerable<TRColour4> colours)
    {
        foreach (TRColour4 colour in colours)
        {
            Write(colour);
        }
    }

    public void Write(TRColour4 colour)
    {
        Write(colour.Red);
        Write(colour.Green);
        Write(colour.Blue);
        Write(colour.Alpha);
    }

    public void Write(IEnumerable<TR1Entity> entities)
    {
        foreach (TR1Entity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR1Entity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.Angle);
        Write(entity.Intensity);
        Write(entity.Flags);
    }

    public void Write(IEnumerable<TR2Entity> entities)
    {
        foreach (TR2Entity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR2Entity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.Angle);
        Write(entity.Intensity1);
        Write(entity.Intensity2);
        Write(entity.Flags);
    }

    public void Write(IEnumerable<TR3Entity> entities)
    {
        foreach (TR3Entity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR3Entity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.Angle);
        Write(entity.Intensity1);
        Write(entity.Intensity2);
        Write(entity.Flags);
    }

    public void Write(IEnumerable<TR4Entity> entities)
    {
        foreach (TR4Entity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR4Entity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.Angle);
        Write(entity.Intensity);
        Write(entity.OCB);
        Write(entity.Flags);
    }

    public void Write(IEnumerable<TR4AIEntity> entities)
    {
        foreach (TR4AIEntity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR4AIEntity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.OCB);
        Write(entity.Flags);
        Write(entity.Angle);
        Write(entity.Box);
    }

    public void Write(IEnumerable<TR5Entity> entities)
    {
        foreach (TR5Entity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR5Entity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.Angle);
        Write(entity.Intensity);
        Write(entity.OCB);
        Write(entity.Flags);
    }

    public void Write(IEnumerable<TR5AIEntity> entities)
    {
        foreach (TR5AIEntity entity in entities)
        {
            Write(entity);
        }
    }

    public void Write(TR5AIEntity entity)
    {
        Write((short)entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.OCB);
        Write(entity.Flags);
        Write(entity.Angle);
        Write(entity.Box);
    }

    public void Write(IEnumerable<TRFace> faces, TRGameVersion version)
    {
        foreach (TRFace face in faces)
        {
            Write(face, version);
        }
    }

    public void Write(TRFace face, TRGameVersion version)
    {
        Debug.Assert(face.Vertices.Count == (int)face.Type);
        Write(face.Vertices);
        WriteFaceTexture(face, version);
    }

    public void Write(IEnumerable<TRMeshFace> faces, TRGameVersion version)
    {
        foreach (TRMeshFace face in faces)
        {
            Write(face, version);
        }
    }

    public void Write(TRMeshFace face, TRGameVersion version)
    {
        Debug.Assert(face.Vertices.Count == (int)face.Type);
        Write(face.Vertices);
        WriteFaceTexture(face, version);
        if (version >= TRGameVersion.TR4)
        {
            Write(face.Effects);
        }
    }

    private void WriteFaceTexture(TRFace face, TRGameVersion version)
    {
        ushort texture = face.Texture;
        if (version >= TRGameVersion.TR3)
        {
            texture &= (ushort)(version == TRGameVersion.TR5 ? 0x3FFF : 0x7FFF);
            if (face.DoubleSided)
            {
                texture |= 0x8000;
            }
            if (face.UnknownFlag && version == TRGameVersion.TR5)
            {
                texture |= 0x4000;
            }
        }
        Write(texture);
    }

    public void Write(FixedFloat32 fixedFloat)
    {
        Write(fixedFloat.Whole);
        Write(fixedFloat.Fraction);
    }

    public void Write(TRBoundingBox box)
    {
        Write(box.MinX);
        Write(box.MaxX);
        Write(box.MinY);
        Write(box.MaxY);
        Write(box.MinZ);
        Write(box.MaxZ);
    }

    public void Write(IEnumerable<TRVertex> vertices)
    {
        foreach (TRVertex vertex in vertices)
        {
            Write(vertex);
        }
    }

    public void Write(TRVertex vertex)
    {
        Write(vertex.X);
        Write(vertex.Y);
        Write(vertex.Z);
    }

    public void Write(TRRoomInfo info, TRGameVersion version)
    {
        Write(info.X);
        if (version == TRGameVersion.TR5)
        {
            Write(0);
        }
        Write(info.Z);
        Write(info.YBottom);
        Write(info.YTop);
    }

    public void Write(IEnumerable<TRRoomPortal> portals)
    {
        foreach (TRRoomPortal portal in portals)
        {
            Write(portal);
        }
    }

    public void Write(TRRoomPortal portal)
    {
        Write(portal.AdjoiningRoom);
        Write(portal.Normal);
        Write(portal.Vertices);
    }

    public void Write(IEnumerable<TRRoomSector> sectors)
    {
        foreach (TRRoomSector sector in sectors)
        {
            Write(sector);
        }
    }

    public void Write(TRRoomSector sector)
    {
        Write(sector.FDIndex);
        Write(sector.BoxIndex);
        Write(sector.RoomBelow);
        Write(sector.Floor);
        Write(sector.RoomAbove);
        Write(sector.Ceiling);
    }

    public void Write(IEnumerable<TRSpriteTexture> textures, TRGameVersion version)
    {
        foreach (TRSpriteTexture texture in textures)
        {
            Write(texture, version);
        }
    }

    public void Write(TRSpriteTexture texture, TRGameVersion version)
    {
        Write(texture.Atlas);
        if (version < TRGameVersion.TR4)
        {
            Write(texture.X);
            Write(texture.Y);
            Write((ushort)(texture.Width * TRConsts.TPageWidth - 1));
            Write((ushort)(texture.Height * TRConsts.TPageHeight - 1));
            Write(texture.Alignment.Left);
            Write(texture.Alignment.Top);
            Write(texture.Alignment.Right);
            Write(texture.Alignment.Bottom);
        }
        else
        {
            Write((byte)texture.Alignment.Left);
            Write((byte)texture.Alignment.Top);
            Write((ushort)((texture.Width - 1) * TRConsts.TPageWidth));
            Write((ushort)((texture.Height - 1) * TRConsts.TPageHeight));
            Write((short)texture.X);
            Write((short)texture.Y);
            Write(texture.Alignment.Right);
            Write(texture.Alignment.Bottom);
        }
    }
}
