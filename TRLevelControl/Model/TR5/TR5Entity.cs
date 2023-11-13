namespace TRLevelControl.Model;

public class TR5Entity : TREntity<TR5Type>
{
    public short Intensity { get; set; }
    public short OCB { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity: {Intensity} OCB: {OCB}";
    }
}
