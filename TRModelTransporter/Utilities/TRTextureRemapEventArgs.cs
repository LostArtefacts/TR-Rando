using System;
using System.Drawing;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public class TRTextureRemapEventArgs : EventArgs
    {
        public TexturedTile OldTile { get; set; }
        public int OldFirstTextureIndex { get; set; }
        public int OldArea { get; set; }
        public Rectangle OldBounds { get; set; }

        public TexturedTile NewTile { get; set; }
        public TexturedTileSegment NewSegment { get; set; }
        public Rectangle NewBounds { get; set; }

        public Point AdjustmentPoint { get; set; }
    }
}