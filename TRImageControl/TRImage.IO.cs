using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using Microsoft.Toolkit.HighPerformance;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using IS = SixLabors.ImageSharp;

namespace TRImageControl;

public partial class TRImage
{
    public TRImage(string filePath)
    {
        ReadFile(filePath, GetImageType(filePath));
    }

    public TRImage(string filePath, ExtImageType type)
    {
        ReadFile(filePath, type);
    }

    public TRImage(Stream stream, ExtImageType type)
    {
        ReadStream(stream, type);
    }

    private static ExtImageType GetImageType(string filePath)
    {
        return Path.GetExtension(filePath).ToUpper() switch
        {
            ".PNG" => ExtImageType.PNG,
            ".DDS" => ExtImageType.DDS,
            _ => throw new NotSupportedException(),
        };
    }

    private void ReadFile(string filePath, ExtImageType type)
    {
        using FileStream stream = File.OpenRead(filePath);
        ReadStream(stream, type);
    }

    private void ReadStream(Stream stream, ExtImageType type)
    {
        switch (type)
        {
            case ExtImageType.PNG:
                ReadPNG(stream);
                break;
            case ExtImageType.DDS:
                ReadDDS(stream);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void ReadPNG(Stream stream)
    {
        using IS.Image<Rgba32> image = IS.Image.Load<Rgba32>(stream);
        ReplaceFrom(image);
    }

    private void ReadDDS(Stream stream)
    {
        BcDecoder decoder = new();
        Memory2D<ColorRgba32> pixels = decoder.Decode2D(stream);
        Span2D<ColorRgba32> span = pixels.Span;

        Size = new(pixels.Width, pixels.Height);
        Pixels = new uint[Size.Width * Size.Height];

        for (int y = 0; y < Height; y++)
        {
            Span<ColorRgba32> row = span.GetRowSpan(y);
            for (int x = 0; x < Width; x++)
            {
                Color c = Color.FromArgb(row[x].a, row[x].r, row[x].g, row[x].b);
                this[x, y] = (uint)c.ToArgb();
            }
        }
    }

    public void Save(string fileName)
    {
        Save(fileName, GetImageType(fileName));
    }

    public void Save(string fileName, ExtImageType type)
    {
        using FileStream fs = File.OpenWrite(fileName);
        Save(fs, type);
    }

    public void Save(Stream stream, ExtImageType type)
    {
        switch (type)
        {
            case ExtImageType.PNG:
                WritePNG(stream);
                break;
            case ExtImageType.DDS:
                WriteDDS(stream);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void WritePNG(Stream stream)
    {
        using IS.Image<Rgba32> image = ToImage();
        image.Save(stream, new PngEncoder());
    }

    public IS.Image<Rgba32> ToImage()
    {
        IS.Image<Rgba32> image = new(Width, Height);
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    Color c = GetPixel(x, y);
                    row[x] = new(c.R, c.G, c.B, c.A);
                }
            }
        });

        return image;
    }

    public void ReplaceFrom(IS.Image<Rgba32> image)
    {
        if (image.Width != Width || image.Height != Height)
        {
            Size = new(image.Width, image.Height);
            Pixels = new uint[Size.Width * Size.Height];
        }

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    Color c = Color.FromArgb(row[x].A, row[x].R, row[x].G, row[x].B);
                    this[x, y] = (uint)c.ToArgb();
                }
            }
        });
    }

    private void WriteDDS(Stream stream)
    {
        ColorRgba32[] colours = new ColorRgba32[Width * Height];
        Read((c, x, y) => colours[y * Width + x] = new(c.R, c.G, c.B, c.A));

        BCnEncoder.Encoder.BcEncoder encoder = new();
        encoder.OutputOptions.GenerateMipMaps = true;
        encoder.OutputOptions.MaxMipMapLevel = 6;
        encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
        encoder.OutputOptions.Format = CompressionFormat.Bc7;

        Memory2D<ColorRgba32> pixels = colours.AsMemory().AsMemory2D(Height, Width);
        DdsFile file = encoder.EncodeToDds(pixels);

        file.Write(stream);
    }
}
