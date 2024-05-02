namespace TRLevelControl.Model;

public class FDBeetleEntry : FDEntry
{
    public override FDFunction GetFunction()
        => FDFunction.MechBeetleOrMinecartRotateRight;

    public override FDEntry Clone()
        => (FDBeetleEntry)MemberwiseClone();
}
