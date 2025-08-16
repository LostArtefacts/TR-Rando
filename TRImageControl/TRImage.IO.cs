using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
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
            ".WEBP" => ExtImageType.WEBP,
            ".JPG" or ".JPEG" => ExtImageType.JPG,
            ".GIF" => ExtImageType.GIF,
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
            case ExtImageType.WEBP:
            case ExtImageType.JPG:
            case ExtImageType.GIF:
                Read(stream);
                break;
            case ExtImageType.DDS:
                ReadDDS(stream);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void Read(Stream stream)
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
                Write(stream, new PngEncoder());
                break;
            case ExtImageType.WEBP:
                Write(stream, new WebpEncoder());
                break;
            case ExtImageType.JPG:
                Write(stream, new JpegEncoder());
                break;
            case ExtImageType.GIF:
                Write(stream, new GifEncoder());
                break;
            case ExtImageType.DDS:
                WriteDDS(stream);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void Write(Stream stream, IImageEncoder encoder)
    {
        using IS.Image<Rgba32> image = ToImage();
        image.Save(stream, encoder);
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
