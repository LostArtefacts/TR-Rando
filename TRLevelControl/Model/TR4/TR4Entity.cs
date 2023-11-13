namespace TRLevelControl.Model;

public class TR4Entity : TREntity<TR4Type>
{
    public short Intensity { get; set; }
    public short OCB { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity: {Intensity} OCB: {OCB}";
    }
}
