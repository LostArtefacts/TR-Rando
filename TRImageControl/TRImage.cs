using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl;

public partial class TRImage : ICloneable
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
                callback.Invoke(GetPixel(x, y), x, y);
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
                this[x, y] = (uint)callback.Invoke(GetPixel(x, y), x, y).ToArgb();
            }
        }

        DataChanged?.Invoke(this, EventArgs.Empty);
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
