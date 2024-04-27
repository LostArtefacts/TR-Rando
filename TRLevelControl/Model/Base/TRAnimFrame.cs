namespace TRLevelControl.Model;

public class TRAnimFrame : ICloneable
{
    public TRBoundingBox Bounds { get; set; }
    public short OffsetX { get; set; }
    public short OffsetY { get; set; }
    public short OffsetZ { get; set; }
    public List<TRAnimFrameRotation> Rotations { get; set; }

    public TRAnimFrame Clone()
    {
        return new()
        {
            Bounds = Bounds.Clone(),
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            OffsetZ = OffsetZ,
            Rotations = new(Rotations.Select(r => r.Clone()))
        };
    }

    object ICloneable.Clone()
        => Clone();
}
