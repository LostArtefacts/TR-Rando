using RectanglePacker;
using RectanglePacker.Organisation;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Packing;

public class DefaultTexturePacker : AbstractPacker<TexturedTile, TexturedTileSegment>, IDisposable
{
    public IReadOnlyList<AbstractIndexedTRTexture> AllTextures => _allTextures;

    private readonly List<AbstractIndexedTRTexture> _allTextures;

    public DefaultTexturePacker()
    {
        TileWidth = 256;
        TileHeight = 256;
        MaximumTiles = 16;

        Options = new PackingOptions
        {
            FillMode = PackingFillMode.Vertical,
            OrderMode = PackingOrderMode.Height,
            Order = PackingOrder.Descending,
            GroupMode = PackingGroupMode.None,
            StartMethod = PackingStartMethod.EndTile
        };

        _allTextures = new List<AbstractIndexedTRTexture>();
    }

    public void Dispose()
    {
        foreach (TexturedTile tile in _tiles)
        {
            tile.Dispose();
        }
    }

    protected override TexturedTile CreateTile()
    {
        return new TexturedTile();
    }
}
