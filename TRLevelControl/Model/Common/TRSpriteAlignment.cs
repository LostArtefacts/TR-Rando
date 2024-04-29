namespace TRLevelControl.Model;

public class TRSpriteAlignment : ICloneable
{
    public short Left { get; set; }
    public short Top { get; set; }
    public short Right { get; set; }
    public short Bottom { get; set; }

    public TRSpriteAlignment Clone()
        => (TRSpriteAlignment)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
