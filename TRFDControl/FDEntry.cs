namespace TRFDControl;

public class FDEntry
{
    public FDSetup Setup { get; set; }

    public virtual ushort[] Flatten()
    {
        return new ushort[] { Setup.Value };
    }
}
