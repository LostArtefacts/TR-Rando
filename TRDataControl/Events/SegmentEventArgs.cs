using TRImageControl;

namespace TRModelTransporter.Events;

public class SegmentEventArgs : EventArgs
{
    public int SegmentIndex { get; set; }
    public TRImage Image { get; set; }
}
