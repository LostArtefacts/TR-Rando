using System.Drawing;

namespace TRImageControl.Packing;

public class PositionedTexture
{
    private readonly TRTextileSegment _texture;

    public int OriginalIndex => _texture.Index;
    public int TileIndex => _texture.Atlas;
    public Point Position => new(_texture.Bounds.X, _texture.Bounds.Y);
    public Rectangle Bounds => _texture.Bounds;

    public PositionedTexture(TRTextileSegment texture)
    {
        _texture = texture;
    }
}
