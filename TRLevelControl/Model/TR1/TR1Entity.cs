namespace TRLevelControl.Model;

public class TR1Entity : TREntity<TR1Type>
{
    public short Intensity { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity: {Intensity}";
    }
}
