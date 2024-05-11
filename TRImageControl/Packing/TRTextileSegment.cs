using System.Drawing;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TRTextileSegment : ICloneable
{
    public TRTexture Texture { get; set; }
    public int Atlas
    {
        get => Texture.Atlas;
        set => Texture.Atlas = (ushort)value;
    }

    public int Index { get; set; }
    public int Area => Bounds.Width * Bounds.Height;
    public Rectangle Bounds => Texture.Bounds;
    public Point Position
    {
        get => Texture.Position;
        set => Texture.Position = value;
    }

    public void Commit(int tileIndex)
    {
        Atlas = tileIndex;
    }

    public void Invalidate()
    {
        Texture.Invalidate();
    }

    public bool IsValid()
    {
        return Texture.IsValid();
    }

    public override string ToString()
    {
        return Bounds.ToString();
    }

    public TRTextileSegment Clone()
    {
        return new()
        {
            Index = Index,
            Texture = Texture.Clone(),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
