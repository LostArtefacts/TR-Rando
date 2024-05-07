using System.Drawing;

namespace TRImageControl.Packing;

public class TextureRemap
{
    public int OriginalTile { get; set; }
    public int OriginalIndex { get; set; }
    public Rectangle OriginalBounds { get; set; }
    public int NewTile { get; set; }
    public int NewIndex { get; set; }
    public Rectangle NewBounds { get; set; }
    public Point AdjustmentPoint { get; set; }
}
