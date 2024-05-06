using RectanglePacker;
using RectanglePacker.Organisation;
using TRLevelControl;

namespace TRImageControl.Packing;

public class DefaultTexturePacker : AbstractPacker<TRTextile, TRTextileRegion>
{
    public DefaultTexturePacker()
    {
        TileWidth = TRConsts.TPageWidth;
        TileHeight = TRConsts.TPageHeight;
        MaximumTiles = 16;

        Options = new()
        {
            FillMode = PackingFillMode.Vertical,
            OrderMode = PackingOrderMode.Height,
            Order = PackingOrder.Descending,
            GroupMode = PackingGroupMode.None,
            StartMethod = PackingStartMethod.EndTile
        };
    }
}
