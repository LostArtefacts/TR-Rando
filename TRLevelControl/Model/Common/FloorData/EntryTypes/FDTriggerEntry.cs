namespace TRLevelControl.Model;

public class FDTriggerEntry : FDEntry
{
    public FDTrigType TrigType { get; set; }
    public byte Timer { get; set; }
    public bool OneShot { get; set; }
    public byte Mask { get; set; } = TRConsts.FullMask;
    public short SwitchOrKeyRef { get; set; }
    public List<FDActionItem> Actions { get; set; } = new();

    public override FDFunction GetFunction()
        => FDFunction.Trigger;

    public override FDEntry Clone()
    {
        return new FDTriggerEntry
        {
            TrigType = TrigType,
            Timer = Timer,
            OneShot = OneShot,
            Mask = Mask,
            SwitchOrKeyRef = SwitchOrKeyRef,
            Actions = new(Actions.Select(a => a.Clone()))
        };
    }
}
