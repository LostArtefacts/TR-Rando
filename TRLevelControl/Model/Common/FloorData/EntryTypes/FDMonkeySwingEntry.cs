namespace TRLevelControl.Model;

public class FDMonkeySwingEntry : FDEntry
{
    public override FDFunction GetFunction()
        => FDFunction.Monkeyswing;

    public override FDEntry Clone()
        => (FDMonkeySwingEntry)MemberwiseClone();
}
