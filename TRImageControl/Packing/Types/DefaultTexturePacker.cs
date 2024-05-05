using RectanglePacker;
using RectanglePacker.Organisation;
using TRLevelControl;

namespace TRImageControl.Packing;

public class DefaultTexturePacker : AbstractPacker<TRTextile, TRTextileRegion>
{
    public IReadOnlyList<TRTextileSegment> AllTextures => _allTextures;

    private readonly List<TRTextileSegment> _allTextures;

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

        _allTextures = new List<TRTextileSegment>();
    }
}
