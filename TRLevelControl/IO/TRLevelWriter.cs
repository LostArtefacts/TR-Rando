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
            Write(colour.Red);
            Write(colour.Green);
            Write(colour.Blue);
            Write(colour.Unused);
        }
    }
}
