using System.Collections.Generic;
using TRFDControl;
using TRFDControl.FDEntryTypes;

namespace TREnvironmentEditor.Helpers
{
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
            FDTriggerEntry entry = new FDTriggerEntry
            {
                Setup = new FDSetup(FDFunctions.Trigger),
                TrigType = TrigType,
                SwitchOrKeyRef = (ushort)levelData.ConvertEntity(SwitchOrKeyRef),
                TrigSetup = new FDTrigSetup
                {
                    OneShot = OneShot,
                    Mask = Mask,
                    Timer = Timer
                },
                TrigActionList = new List<FDActionListItem>()
            };

            foreach (EMTriggerAction action in Actions)
            {
                entry.TrigActionList.Add(action.ToFDAction(levelData));
            }

            return entry;
        }

        public static EMTrigger FromFDEntry(FDTriggerEntry entry)
        {
            EMTrigger trigger = new EMTrigger
            {
                TrigType = entry.TrigType,
                OneShot = entry.TrigSetup.OneShot,
                Mask = entry.TrigSetup.Mask,
                Timer = entry.TrigSetup.Timer,
                SwitchOrKeyRef = (short)entry.SwitchOrKeyRef,
                Actions = new List<EMTriggerAction>()
            };

            foreach (FDActionListItem action in entry.TrigActionList)
            {
                trigger.Actions.Add(EMTriggerAction.FromFDAction(action));
            }

            return trigger;
        }
    }
}