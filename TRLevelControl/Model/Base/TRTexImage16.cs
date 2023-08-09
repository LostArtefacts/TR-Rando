using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Helpers;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRTexImage16 : ISerializableCompact
{
    public ushort[] Pixels { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (ushort pixel in Pixels)
                {
                    writer.Write(pixel);
                }
            }

            return stream.ToArray();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());

        sb.Append("\n");

        int Count = 1;
        foreach (ushort pixel in Pixels)
        {
            sb.Append(pixel + " ");

            Count++;

            if (Count % 8 == 0)
            {
                sb.Append("\n");
            }
        }

        return sb.ToString();
    }

    public Textile16Pixel[] To32BPPFormat()
    {
        Textile16Pixel[] pixels = new Textile16Pixel[256 * 256];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Textile16Pixel { Value = this.Pixels[i] };
        }

        return pixels;
    }
}

//256 * 256
//1-bit transparency (0 = transparent, 1 = opaque) (0x8000)
//5-bit red channel(0x7C00)
//5-bit green channel(0x03E0)
//5-bit blue channel(0x001F)
public class Textile16Pixel
{
    public ushort Value { get; set; }

    private byte _Red 
    { 
        get
        {
            return Convert.ToByte((Value & 0x7C00) >> 10);
        }
    }

    public byte Red
    {
        get
        {
            return TextileToBitmapConverter.To32BPP(_Red);
        }
    }

    private byte _Blue 
    { 
        get
        {
            return Convert.ToByte(Value & 0x001F);
        }
    }

    public byte Blue
    {
        get
        {
            return TextileToBitmapConverter.To32BPP(_Blue);
        }
    }

    private byte _Green 
    { 
        get
        {
            return Convert.ToByte((Value & 0x03E0) >> 5);
        }
    }

    public byte Green
    {
        get
        {
            return TextileToBitmapConverter.To32BPP(_Green);
        }
    }

    private byte _Transparent 
    { 
        get
        {
            return Convert.ToByte((Value & 0x8000) >> 15);
        }
    }

    public byte Transparency
    {
        get
        {
            if (_Transparent == 0x1)
            {
                return 0xFF;
            }
            else
            {
                return 0x00;
            }
        }
    }

    public byte[] RGB32
    {
        get
        {
            return new byte[] { Blue, Green, Red, Transparency };
        }
    }

    public Textile16Pixel()
    {
        
    }
}
