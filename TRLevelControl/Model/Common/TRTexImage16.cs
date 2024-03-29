﻿using TRLevelControl.Helpers;

namespace TRLevelControl.Model;

public class TRTexImage16 : TRTexImage<ushort>
{
    public Textile16Pixel[] To32BPPFormat()
    {
        Textile16Pixel[] pixels = new Textile16Pixel[TRConsts.TPageSize];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Textile16Pixel { Value = Pixels[i] };
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

    public byte Red
    {
        get
        {
            byte red = Convert.ToByte((Value & 0x7C00) >> 10);
            return TextileToBitmapConverter.To32BPP(red);
        }
    }

    public byte Blue
    {
        get
        {
            byte blue = Convert.ToByte(Value & 0x001F);
            return TextileToBitmapConverter.To32BPP(blue);
        }
    }

    public byte Green
    {
        get
        {
            byte green = Convert.ToByte((Value & 0x03E0) >> 5);
            return TextileToBitmapConverter.To32BPP(green);
        }
    }

    public byte Transparency
    {
        get
        {
            if (Convert.ToByte((Value & 0x8000) >> 15) == 0x1)
            {
                return 0xFF;
            }
            return 0x00;
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
