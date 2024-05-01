namespace TRLevelControl.Model;

public class FDTriggerEntry : FDEntry
{
    public FDTriggerEntry()
    {
        TrigActionList = new List<FDActionItem>();
    }

    public FDTrigSetup TrigSetup { get; set; }

    public FDTrigType TrigType
    {
        get
        {
            return (FDTrigType)Setup.SubFunction;
        }
        set
        {
            Setup.SubFunction = (byte)value;
        }
    }

    public List<FDActionItem> TrigActionList { get; set; }

    public ushort SwitchOrKeyRef { get; set; }

    public bool SwitchKeyContinue
    {
        get
        {
            //Continue bit set to 0 means to continue, not 1...
            return !((SwitchOrKeyRef & 0x8000) > 0);
        }
        internal set
        {
            if (value)
            {
                SwitchOrKeyRef = (ushort)(SwitchOrKeyRef & ~0x8000);
            }
            else
            {
                SwitchOrKeyRef |= 0x8000;
            }
        }
    }

    public override ushort[] Flatten()
    {
        //FD Setup followed by TrigSetup
        List<ushort> dataArray = new()
        {
            Setup.Value,
            TrigSetup.Value
        };

        //If key or switch, next uint16 will be ref to key or switch ent
        if (TrigType == FDTrigType.Switch || TrigType == FDTrigType.Key)
            dataArray.Add(SwitchOrKeyRef);


        for (int i = 0; i < TrigActionList.Count; i++)
        {
            FDActionItem action = TrigActionList[i];

            // Ensure Continue is set on all but the final action, unless it's a camera action,
            // in which case the CamAction will have Continue set to false if this is the final action.
            action.Continue = (action.TrigAction == FDTrigAction.Camera || action.TrigAction == FDTrigAction.Flyby) || i < TrigActionList.Count - 1;

            //For each action, record the value.
            dataArray.Add(action.Value);

            //If it is a camera action, record the additional camera action
            if (action.TrigAction == FDTrigAction.Camera || action.TrigAction == FDTrigAction.Flyby)
            {
                action.CamAction.Continue = i < TrigActionList.Count - 1;
                dataArray.Add(action.CamAction.Value);
            }
        }

        //Return uint16 array
        return dataArray.ToArray();
    }
}
