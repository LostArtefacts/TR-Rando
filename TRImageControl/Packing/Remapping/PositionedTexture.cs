using System.Drawing;

namespace TRImageControl.Packing;

public class PositionedTexture
{
    private readonly TRTextileSegment _segment;

    public int OriginalIndex => _segment.Index;
    public int TileIndex => _segment.Atlas;
    public Point Position => new(_segment.Bounds.X, _segment.Bounds.Y);
    public Rectangle Bounds => _segment.Bounds;

    public PositionedTexture(TRTextileSegment segment)
    {
        _segment = segment;
    }
}
