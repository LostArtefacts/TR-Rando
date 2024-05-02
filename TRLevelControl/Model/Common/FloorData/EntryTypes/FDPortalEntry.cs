namespace TRLevelControl.Model;

public class FDPortalEntry : FDEntry
{
    public short Room { get; set; }

    public override FDFunction GetFunction()
        => FDFunction.PortalSector;

    public override FDEntry Clone()
        => (FDPortalEntry)MemberwiseClone();
}
