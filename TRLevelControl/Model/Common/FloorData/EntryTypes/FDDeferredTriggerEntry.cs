namespace TRLevelControl.Model;

public class FDDeferredTriggerEntry : FDEntry
{
    public override FDFunction GetFunction()
        => FDFunction.DeferredTrigOrMinecartRotateLeft;

    public override FDEntry Clone()
        => (FDDeferredTriggerEntry)MemberwiseClone();
}
