namespace TRLevelControl.Model;

public class TRBoundingBox : ICloneable
{
    public short MinX { get; set; }
    public short MaxX { get; set; }
    public short MinY { get; set; }
    public short MaxY { get; set; }
    public short MinZ { get; set; }
    public short MaxZ { get; set; }

    public TRBoundingBox Clone()
    {
        return new()
        {
            MinX = MinX,
            MaxX = MaxX,
            MinY = MinY,
            MaxY = MaxY,
            MinZ = MinZ,
            MaxZ = MaxZ
        };
    }

    object ICloneable.Clone()
        => Clone();
}
