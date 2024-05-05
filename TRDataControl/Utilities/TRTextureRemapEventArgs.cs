using System.Drawing;
using TRImageControl.Packing;

namespace TRModelTransporter.Utilities;

public class TRTextureRemapEventArgs : EventArgs
{
    public TRTextile OldTile { get; set; }
    public int OldFirstTextureIndex { get; set; }
    public int OldArea { get; set; }
    public Rectangle OldBounds { get; set; }

    public TRTextile NewTile { get; set; }
    public TRTextileRegion NewSegment { get; set; }
    public Rectangle NewBounds { get; set; }

    public Point AdjustmentPoint { get; set; }
}
