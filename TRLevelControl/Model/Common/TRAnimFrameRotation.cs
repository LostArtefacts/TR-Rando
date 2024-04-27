namespace TRLevelControl.Model;

public class TRAnimFrameRotation : ICloneable
{
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public TRAnimFrameRotation Clone()
        => (TRAnimFrameRotation)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
