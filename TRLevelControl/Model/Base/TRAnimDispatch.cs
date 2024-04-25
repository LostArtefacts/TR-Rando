namespace TRLevelControl.Model;

public class TRAnimDispatch : ICloneable
{
    public short Low { get; set; }
    public short High { get; set; }
    public short NextAnimation { get; set; }
    public short NextFrame { get; set; }

    public TRAnimDispatch Clone()
    {
        return new()
        {
            Low = Low,
            High = High,
            NextAnimation = NextAnimation,
            NextFrame = NextFrame
        };
    }

    object ICloneable.Clone()
        => Clone();
}
