namespace TRLevelControl.Model;

public class FDKillLaraEntry : FDEntry
{
    public override FDFunction GetFunction()
        => FDFunction.KillLara;

    public override FDEntry Clone()
        => (FDKillLaraEntry)MemberwiseClone();
}
