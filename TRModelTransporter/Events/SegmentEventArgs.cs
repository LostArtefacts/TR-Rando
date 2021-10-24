using System;
using System.Drawing;

namespace TRModelTransporter.Events
{
    public class SegmentEventArgs : EventArgs
    {
        public int SegmentIndex { get; set; }
        public Bitmap Bitmap { get; set; }
    }
}