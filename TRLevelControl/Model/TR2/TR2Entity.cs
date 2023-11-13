namespace TRLevelControl.Model;

public class TR2Entity : TREntity<TR2Type>
{
    public short Intensity1 { get; set; }
    public short Intensity2 { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} Intensity1: {Intensity1} Intensity2: {Intensity2}";
    }
}
