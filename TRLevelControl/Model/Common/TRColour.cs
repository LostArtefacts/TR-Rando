namespace TRLevelControl.Model;

public class TRColour : ICloneable
{
    public byte Red { get; set; }
    public byte Green { get; set; }    
    public byte Blue { get; set; }

    public TRColour Clone()
        => (TRColour)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
