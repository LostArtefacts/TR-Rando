using System.Drawing;

namespace TRLevelControl.Model;

public class TRSpriteTexture : TRTexture
{
    public byte X { get; set; }
    public byte Y { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public TRSpriteAlignment Alignment { get; set; }

    protected override Rectangle GetBounds()
    {
        return new(X, Y, Width, Height);
    }

    protected override void SetPosition(Point position)
    {
        X = (byte)position.X;
        Y = (byte)position.Y;
    }

    protected override void SetSize(Size size)
    {
        Width = (ushort)size.Width;
        Height = (ushort)size.Height;
    }

    public override TRSpriteTexture Clone()
    {
        return new()
        {
            Atlas = Atlas,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Alignment = Alignment?.Clone(),
        };
    }
}
