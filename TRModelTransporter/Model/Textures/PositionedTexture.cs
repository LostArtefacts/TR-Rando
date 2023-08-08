using System.Drawing;

namespace TRModelTransporter.Model.Textures;

public class PositionedTexture
{
    private readonly AbstractIndexedTRTexture _texture;

    public int OriginalIndex => _texture.Index;
    public int TileIndex => _texture.Atlas;
    public Point Position => new Point(_texture.Bounds.X, _texture.Bounds.Y);
    public Rectangle Bounds => _texture.Bounds;

    public PositionedTexture(AbstractIndexedTRTexture texture)
    {
        _texture = texture;
    }
}
