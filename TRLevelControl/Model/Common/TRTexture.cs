using System.Drawing;

namespace TRLevelControl.Model;

public abstract class TRTexture : ICloneable
{
    public ushort Atlas { get; set; }
    public Rectangle Bounds
    {
        get => GetBounds();
        set
        {
            Position = value.Location;
            Size = value.Size;
        }
    }

    public Point Position
    {
        get => Bounds.Location;
        set => SetPosition(value);
    }

    public Size Size
    {
        get => Bounds.Size;
        set => SetSize(value);
    }

    public List<Point> Points => GetPoints();

    protected List<Point> GetPoints()
    {
        Rectangle bounds = GetBounds();
        return new()
        {
            new() { X = bounds.Left, Y = bounds.Top },
            new() { X = bounds.Right - 1, Y = bounds.Top },
            new() { X = bounds.Right - 1, Y = bounds.Bottom - 1 },
            new() { X = bounds.Left, Y = bounds.Bottom - 1 },
        };
    }

    protected abstract Rectangle GetBounds();
    protected abstract void SetPosition(Point position);
    protected abstract void SetSize(Size size);

    public abstract TRTexture Clone();

    object ICloneable.Clone()
        => Clone();
}
