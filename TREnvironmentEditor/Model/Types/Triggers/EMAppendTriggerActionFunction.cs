using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAppendTriggerActionFunction : BaseEMFunction
    {
        public EMLocation Location { get; set; }
        public List<EMTriggerAction> Actions { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, control);
            if (sector.FDIndex == 0)
            {
                return;
            }

            FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (trigger != null)
            {
                trigger.TrigActionList.AddRange(actions);
                control.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, control);
            if (sector.FDIndex == 0)
            {
                return;
            }

            FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (trigger != null)
            {
                trigger.TrigActionList.AddRange(actions);
                control.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, control);
            if (sector.FDIndex == 0)
            {
                return;
            }

            FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (trigger != null)
            {
                trigger.TrigActionList.AddRange(actions);
                control.WriteToLevel(level);
            }
        }

        private List<FDActionListItem> InitialiseActionItems(EMLevelData data)
        {
            List<FDActionListItem> actions = new List<FDActionListItem>();
            foreach (EMTriggerAction action in Actions)
            {
                actions.Add(action.ToFDAction(data));
            }
            return actions;
        }
    }
}