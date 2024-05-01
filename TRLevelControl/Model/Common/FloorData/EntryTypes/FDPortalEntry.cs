namespace TRLevelControl.Model;

public class FDPortalEntry : FDEntry
{
    public ushort Room { get; set; }

    public override ushort[] Flatten()
    {
        return new ushort[]
        {
            Setup.Value,
            Room
        };
    }
}
