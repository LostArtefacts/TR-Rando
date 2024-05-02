using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

// This is redundant now - to be eliminated
public class EMTrigger
{
    private static readonly byte _defaultMask = 31;

    public FDTrigType TrigType { get; set; }
    public bool OneShot { get; set; }
    public byte Mask { get; set; }
    public byte Timer { get; set; }
    public short SwitchOrKeyRef { get; set; }
    public List<EMTriggerAction> Actions { get; set; }

    public EMTrigger()
    {
        Mask = _defaultMask;
    }

    public FDTriggerEntry ToFDEntry(EMLevelData levelData)
    {
        FDTriggerEntry entry = new()
        {
            TrigType = TrigType,
            SwitchOrKeyRef = levelData.ConvertEntity(SwitchOrKeyRef),
            OneShot = OneShot,
            Mask = Mask,
            Timer = Timer,
            Actions = new()
        };

        foreach (EMTriggerAction action in Actions)
        {
            entry.Actions.Add(action.ToFDAction(levelData));
        }

        return entry;
    }

    public static EMTrigger FromFDEntry(FDTriggerEntry entry)
    {
        EMTrigger trigger = new()
        {
            TrigType = entry.TrigType,
            OneShot = entry.OneShot,
            Mask = entry.Mask,
            Timer = entry.Timer,
            SwitchOrKeyRef = entry.SwitchOrKeyRef,
            Actions = new()
        };

        foreach (FDActionItem action in entry.Actions)
        {
            trigger.Actions.Add(EMTriggerAction.FromFDAction(action));
        }

        return trigger;
    }
}
