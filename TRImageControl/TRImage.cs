using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TRImageControl.Textures;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl;

public class TRImage : ICloneable
{
    private const uint _trImageMagic = 'T' | ('R' << 8) | ('I' << 16) | ('M' << 24);
    private const uint _sizeDelimiter = 'S' | ('I' << 8) | ('Z' << 16) | ('E' << 24);
    private const uint _dataDelimiter = 'D' | ('A' << 8) | ('T' << 16) | ('A' << 24);

    public uint[] Pixels { get; set; }
    public Size Size { get; set; }
    public int Width => Size.Width;
    public int Height => Size.Height;

    public uint this[int x, int y]
    {
        get => Pixels[y * Size.Width + x];
        set => Pixels[y * Size.Width + x] = value;
    }

    public event EventHandler DataChanged;

    public TRImage()
        : this(TRConsts.TPageWidth, TRConsts.TPageHeight) { }

    public TRImage(int width, int height)
        : this(new Size(width, height)) { }

    public TRImage(Size size)
    {
        Size = size;
        Pixels = new uint[size.Width * size.Height];
    }

    public TRImage(byte[] pixels, List<TRColour> palette)
        : this(new(TRConsts.TPageWidth, TRConsts.TPageHeight), pixels, palette) { }

    public TRImage(Size size, byte[] pixels, List<TRColour> palette)
        : this(size)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            byte index = pixels[i];
            TRColour colour = palette[index];
            Pixels[i] = colour.Blue;
            Pixels[i] |= (uint)(colour.Green << 8);
            Pixels[i] |= (uint)(colour.Red << 16);
            Pixels[i] |= (uint)((index == 0 ? 0 : 0xFF) << 24);
        }
    }

    public TRImage(ushort[] pixels)
        : this(new(TRConsts.TPageWidth, TRConsts.TPageHeight), pixels) { }

    public TRImage(Size size, ushort[] pixels)
        : this(size)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            ushort rgb555 = pixels[i];
            int a = ((rgb555 & 0x8000) >> 15) > 0 ? 0xFF : 0;
            int r = (rgb555 & 0x7C00) >> 10;
            int g = (rgb555 & 0x03E0) >> 5;
            int b = rgb555 & 0x001F;

            r = r * 255 / 31;
            g = g * 255 / 31;
            b = b * 255 / 31;

            Pixels[i] = (uint)b;
            Pixels[i] |= (uint)(g << 8);
            Pixels[i] |= (uint)(r << 16);
            Pixels[i] |= (uint)(a << 24);
        }
    }

    public TRImage(uint[] pixels)
        : this(new(TRConsts.TPageWidth, TRConsts.TPageHeight), pixels) { }

    public TRImage(Size size, uint[] pixels)
    {
        Size = size;
        Pixels = pixels;
    }

    public TRImage(string filePath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using Bitmap bmp = new(Image.FromStream(fs));
        ReadBitmap(bmp);
    }

    public TRImage(Bitmap bmp)
    {
        ReadBitmap(bmp);
    }

    private void ReadBitmap(Bitmap bmp)
    {
        Size = bmp.Size;
        Pixels = new uint[Size.Width * Size.Height];

        BitmapData bd = bmp.LockBits(new(0, 0, Size.Width, Size.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
        int bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
        int byteCount = bd.Stride * Size.Height;
        byte[] pixels = new byte[byteCount];
        IntPtr ptrFirstPixel = bd.Scan0;
        Marshal.Copy(ptrFirstPixel, pixels, 0, byteCount);

        int endX = Size.Width * bytesPerPixel;
        for (int y = 0; y < Size.Height; y++)
        {
            int currentLine = y * bd.Stride;
            for (int x = 0; x < endX; x += bytesPerPixel)
            {
                uint c = 0;
                for (int i = 0; i < bytesPerPixel; i++)
                {
                    c |= (uint)(pixels[currentLine + x + i] << (i * 8));
                }

                this[x / bytesPerPixel, y] = c;
            }
        }

        bmp.UnlockBits(bd);
    }

    public byte[] ToRGB(List<TRColour> palette)
    {
        byte[] data = new byte[Pixels.Length];
        for (int i = 0; i < Pixels.Length; i++)
        {
            uint argb = Pixels[i];
            byte a = (byte)((argb & 0xFF000000) >> 24);
            if (a == 0)
            {
                data[i] = 0;
            }
            else
            {
                byte r = (byte)((argb & 0xFF0000) >> 16);
                byte g = (byte)((argb & 0xFF00) >> 8);
                byte b = (byte)(argb & 0xFF);
                data[i] = (byte)palette.FindIndex(c => c.Red == r && c.Green == g && c.Blue == b);
            }
        }
        return data;
    }

    public ushort[] ToRGB555()
    {
        ushort[] data = new ushort[Pixels.Length];
        for (int i = 0; i < Pixels.Length; i++)
        {
            uint argb = Pixels[i];
            uint a = (argb >> 16) & 0x8000;
            uint r = (argb >> 9) & 0x7C00;
            uint g = (argb >> 6) & 0x03E0;
            uint b = (argb >> 3) & 0x1F;

            data[i] = (ushort)(a | r | g | b);
        }

        return data;
    }

    public uint[] ToRGB32()
    {
        uint[] copy = new uint[Pixels.Length];
        Array.Copy(Pixels, copy, Pixels.Length);
        return copy;
    }

    public Bitmap ToBitmap()
    {
        List<byte> pixels = new(Pixels.Length * 4);
        for (int i = 0; i < Pixels.Length; i++)
        {
            uint pixel = Pixels[i];
            byte r = (byte)((pixel & 0xFF0000) >> 16);
            byte g = (byte)((pixel & 0xFF00) >> 8);
            byte b = (byte)(pixel & 0xFF);
            byte a = (byte)((pixel & 0xFF000000) > 0 ? 0xFF : 0);

            pixels.Add(b);
            pixels.Add(g);
            pixels.Add(r);
            pixels.Add(a);
        }

        Bitmap bmp = new(Size.Width, Size.Height, PixelFormat.Format32bppArgb);
        BitmapData bitmapData = bmp.LockBits(new(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        Marshal.Copy(pixels.ToArray(), 0, bitmapData.Scan0, pixels.Count);
        bmp.UnlockBits(bitmapData);

        return bmp;
    }

    public void Save(string fileName)
        => Save(fileName, ImageFormat.Png);

    public void Save(string fileName, ImageFormat format)
    {
        using FileStream fs = File.OpenWrite(fileName);
        ToBitmap().Save(fs, format);
    }

    public void Save(Stream stream, ImageFormat format)
        => ToBitmap().Save(stream, format);

    public Color GetPixel(int x, int y)
        => Color.FromArgb((int)this[x, y]);

    public void Read(Action<Color, int, int> callback)
    {
        Read(new(0, 0, Size.Width, Size.Height), callback);
    }

    public void Read(Rectangle bounds, Action<Color, int, int> callback)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                callback.Invoke(Color.FromArgb((int)this[x, y]), x, y);
            }
        }
    }

    public void Write(Func<Color, int, int, Color> callback)
    {
        Write(new(0, 0, Size.Width, Size.Height), callback);
    }

    public void Write(Rectangle bounds, Func<Color, int, int, Color> callback)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                this[x, y] = (uint)callback.Invoke(Color.FromArgb((int)this[x, y]), x, y).ToArgb();
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Import(TRImage image, Point point, bool retainBackground = false)
    {
        if (!retainBackground)
        {
            Delete(new(point, image.Size));
        }

        for (int y = 0; y < image.Size.Height; y++)
        {
            for (int x = 0; x < image.Size.Width; x++)
            {
                Color c = image.GetPixel(x, y);
                if (!retainBackground || c.A != 0)
                {
                    this[x + point.X, y + point.Y] = image[x, y];
                }
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
    }


    public TRImage Export(Rectangle bounds)
    {
        TRImage image = new(bounds.Size);
        for (int y = 0; y < bounds.Height; y++)
        {
            for (int x = 0; x < bounds.Width; x++)
            {
                image[x, y] = this[x + bounds.Left, y + bounds.Top];
            }
        }

        return image;
    }

    public void Delete(Rectangle bounds)
    {
        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                this[x, y] = 0;
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AdjustHSB(Rectangle bounds, HSBOperation operation)
    {
        Write(bounds, (c, x, y) => ApplyHSBOperation(c, operation));
    }

    private static Color ApplyHSBOperation(Color c, HSBOperation operation)
    {
        HSB hsb = c.ToHSB();
        hsb.H = operation.ModifyHue(hsb.H);
        hsb.S = operation.ModifySaturation(hsb.S);
        hsb.B = operation.ModifyBrightness(hsb.B);

        return hsb.ToColour();
    }

    public void Replace(Rectangle bounds, Color search, Color replacement)
    {
        Write(bounds, (c, x, y) => c == search ? replacement : c);
    }

    public void ImportSegment(TRImage source, StaticTextureTarget target, Rectangle sourceSegment)
    {
        Rectangle sourceRectangle = sourceSegment;
        if (target.ClipRequired)
        {
            sourceRectangle.X += target.Clip.X;
            sourceRectangle.Y += target.Clip.Y;
            sourceRectangle.Width = target.Clip.Width;
            sourceRectangle.Height = target.Clip.Height;
        }

        Import(source.Export(sourceRectangle), new(target.X, target.Y), !target.Clear);
    }

    public void Fill(Color colour)
    {
        Write((x, y, c) => colour);
    }

    public void Fill(Rectangle bounds, Color colour)
    {
        Write(bounds, (x, y, c) => colour);
    }

    public void Overlay(TRImage image)
    {
        if (image.Size.Width > Size.Width || image.Size.Height > Size.Height)
        {
            throw new InvalidOperationException();
        }
        Import(image, new(0, 0), true);
    }

    public string GenerateID()
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        using MD5 md5 = MD5.Create();

        writer.Write(_trImageMagic);
        writer.Write(_sizeDelimiter);
        writer.Write(Size.Width);
        writer.Write(Size.Height);

        writer.Write(_dataDelimiter);
        foreach (uint pixel in Pixels)
        {
            writer.Write(pixel);
        }

        byte[] hash = md5.ComputeHash(ms.ToArray());
        StringBuilder sb = new();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public override bool Equals(object obj)
    {
        return obj is TRImage image
            && Size == image.Size
            && Pixels.SequenceEqual(image.Pixels);
    }

    public override int GetHashCode()
    {
        return Pixels.GetHashCode() * Size.GetHashCode();
    }

    public TRImage Clone()
    {
        TRImage image = new(Size);
        for (int i = 0; i < Pixels.Length; i++)
        {
            image.Pixels[i] = Pixels[i];
        }
        return image;
    }

    object ICloneable.Clone()
        => Clone();
}
