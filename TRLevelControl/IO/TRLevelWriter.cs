using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TRLevelWriter : BinaryWriter
{
    public TRLevelWriter()
        : base(new MemoryStream()) { }

    public TRLevelWriter(Stream stream)
        : base(stream) { }

    public void Deflate(TRLevelWriter inflatedWriter, TR4Chunk chunk)
    {
        using MemoryStream outStream = new();
        using DeflaterOutputStream deflater = new(outStream);

        long position = inflatedWriter.BaseStream.Position;
        try
        {
            inflatedWriter.BaseStream.Position = 0;
            inflatedWriter.BaseStream.CopyTo(deflater);
            deflater.Finish();

            byte[] zippedData = outStream.ToArray();
            chunk.UncompressedSize = (uint)inflatedWriter.BaseStream.Length;
            chunk.CompressedSize = (uint)zippedData.Length;

            Write(chunk.UncompressedSize);
            Write(chunk.CompressedSize);
            Write(zippedData);
        }
        finally
        {
            inflatedWriter.BaseStream.Position = position;
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
            Write(image.Pixels);
        }
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
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
        Write(entity.TypeID);
        Write(entity.Room);
        Write(entity.X);
        Write(entity.Y);
        Write(entity.Z);
        Write(entity.OCB);
        Write(entity.Flags);
        Write(entity.Angle);
        Write(entity.Box);
    }
}
