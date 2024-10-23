using System.Drawing;
using TRImageControl.Textures;

namespace TRImageControl;

public partial class TRImage
{
    public void Import(TRImage image, Point point, bool retainBackground = false)
    {
        if (!retainBackground)
        {
            Delete(new(point, image.Size));
        }

        image.Read((c, x, y) =>
        {
            if (!retainBackground || c.A == 0xFF)
            {
                this[x + point.X, y + point.Y] = image[x, y];
            }
            else if (c.A > 0)
            {
                Color curColour = GetPixel(x + point.X, y + point.Y);

                float a0 = c.A / 255.0f;
                float r0 = (c.R / 255.0f) * a0;
                float g0 = (c.G / 255.0f) * a0;
                float b0 = (c.B / 255.0f) * a0;

                float a1 = curColour.A / 255.0f;
                float r1 = (curColour.R / 255.0f) * a1;
                float g1 = (curColour.G / 255.0f) * a1;
                float b1 = (curColour.B / 255.0f) * a1;

                float aOut = a0 + a1 * (1 - a0);
                float rOut = (r0 + r1 * (1 - a0)) / aOut;
                float gOut = (g0 + g1 * (1 - a0)) / aOut;
                float bOut = (b0 + b1 * (1 - a0)) / aOut;

                Color blend = Color.FromArgb((int)(aOut * 255), (int)(rOut * 255), (int)(gOut * 255), (int)(bOut * 255));
                this[x + point.X, y + point.Y] = (uint)blend.ToArgb();
            }
        });

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
            image = image.Export(new(0, 0, Math.Min(image.Size.Width, Size.Width), Math.Min(image.Size.Height, Size.Height)));
        }
        Import(image, new(0, 0), true);
    }
}
