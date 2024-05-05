using RectanglePacker;
using RectanglePacker.Organisation;
using TRLevelControl;

namespace TRImageControl.Packing;

public class DefaultTexturePacker : AbstractPacker<TexturedTile, TexturedTileSegment>
{
    public IReadOnlyList<AbstractIndexedTRTexture> AllTextures => _allTextures;

    private readonly List<AbstractIndexedTRTexture> _allTextures;

    public DefaultTexturePacker()
    {
        TileWidth = TRConsts.TPageWidth;
        TileHeight = TRConsts.TPageHeight;
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

    protected override TexturedTile CreateTile()
    {
        return new TexturedTile();
    }
}
