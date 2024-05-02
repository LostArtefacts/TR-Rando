namespace TRLevelControl.Model;

public class FDMinecartEntry : FDEntry
{
    public FDMinecartType Type { get; set; }

    public override FDFunction GetFunction()
        => (FDFunction)Type;

    public override FDEntry Clone()
        => (FDMinecartEntry)MemberwiseClone();
}
