using System.Drawing;

namespace TRImageControl.Packing;

public class PositionedTexture
{
    public int OriginalIndex { get; set; }
    public int TileIndex { get; set; }
    public Point Position { get; set; }
}
