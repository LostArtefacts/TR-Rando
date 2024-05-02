namespace TRLevelControl.Model;

public class FDSlantEntry : FDEntry
{
    public FDSlantType Type { get; set; }
    public sbyte XSlant { get; set; }
    public sbyte ZSlant { get; set; }

    public override FDFunction GetFunction()
        => (FDFunction)Type;

    public override FDEntry Clone()
        => (FDSlantEntry)MemberwiseClone();
}
