using System;
using System.Drawing;

namespace TRTexture16Importer.Helpers
{
    //https://www.codeproject.com/Articles/19045/Manipulating-colors-in-NET-Part-1
    public static class ColourExtensions
    {
        public static HSB ToHSB(this Color c)
        {
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;

            double minValue = Math.Min(r, Math.Min(g, b));
            double maxValue = Math.Max(r, Math.Max(g, b));
            double delta = maxValue - minValue;

            double hue = 0;
            double saturation;
            double brightness = maxValue * 100;

            if (Math.Abs(maxValue - 0) < 0.0001 || Math.Abs(delta - 0) < 0.0001)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                if (Math.Abs(minValue - 0) < 0.0001)
                {
                    saturation = 100;
                }
                else
                {
                    saturation = (delta / maxValue) * 100;
                }

                if (Math.Abs(r - maxValue) < double.Epsilon)
                {
                    hue = (g - b) / delta;
                }
                else if (Math.Abs(g - maxValue) < double.Epsilon)
                {
                    hue = 2 + (b - r) / delta;
                }
                else if (Math.Abs(b - maxValue) < double.Epsilon)
                {
                    hue = 4 + (r - g) / delta;
                }
            }

            hue *= 60;
            if (hue < 0)
            {
                hue += 360;
            }
            else if (hue > 360)
            {
                hue -= 360;
            }

            return new HSB
            (
                (int)Math.Round(hue, MidpointRounding.AwayFromZero),
                (int)Math.Round(saturation, MidpointRounding.AwayFromZero),
                (int)Math.Round(brightness, MidpointRounding.AwayFromZero),
                c.A
            );
        }

        public static Color ToColour(this HSB hsb)
        {
            double red = 0, green = 0, blue = 0;

            double h = hsb.H;
            double s = hsb.S / 100.0;
            double b = hsb.B / 100.0;

            if (Math.Abs(s - 0) < 0.0001)
            {
                red = b;
                green = b;
                blue = b;
            }
            else
            {
                double sectorPosition = h / 60;
                int sectorNumber = (int)Math.Floor(sectorPosition);
                double fractionalSector = sectorPosition - sectorNumber;

                double p = b * (1 - s);
                double q = b * (1 - (s * fractionalSector));
                double t = b * (1 - (s * (1 - fractionalSector)));

                switch (sectorNumber)
                {
                    case 0:
                        red = b;
                        green = t;
                        blue = p;
                        break;
                    case 1:
                        red = q;
                        green = b;
                        blue = p;
                        break;
                    case 2:
                        red = p;
                        green = b;
                        blue = t;
                        break;
                    case 3:
                        red = p;
                        green = q;
                        blue = b;
                        break;
                    case 4:
                        red = t;
                        green = p;
                        blue = b;
                        break;
                    case 5:
                        red = b;
                        green = p;
                        blue = q;
                        break;
                }
            }

            return Color.FromArgb
            (
                hsb.A,
                (int)Math.Round(red * 255, MidpointRounding.AwayFromZero),
                (int)Math.Round(green * 255, MidpointRounding.AwayFromZero),
                (int)Math.Round(blue * 255, MidpointRounding.AwayFromZero)
            );
        }

        public static ushort ToRGB555(this Color c)
        {
            int argb = c.ToArgb();
            int a = (argb >> 16) & 0x8000;
            int r = (argb >> 9) & 0x7C00;
            int g = (argb >> 6) & 0x03E0;
            int b = (argb >> 3) & 0x1F;

            return (ushort)(a | r | g | b);
        }
    }
}