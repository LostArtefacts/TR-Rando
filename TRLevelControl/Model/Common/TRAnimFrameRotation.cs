namespace TRLevelControl.Model;

public class TRAnimFrameRotation : ICloneable
{
    public TRAngleMode Mode { get; set; } = TRAngleMode.Auto;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public TRAnimFrameRotation Clone()
        => (TRAnimFrameRotation)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
