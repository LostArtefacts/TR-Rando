using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRemoveTriggerActionFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public EMTriggerAction ActionItem { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveAction(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveAction(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                RemoveAction(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        private FDActionListItem InitialiseActionItem(EMLevelData data)
        {
            return ActionItem.ToFDAction(data);
        }

        private void RemoveAction(TRRoomSector sector, FDControl control, FDActionListItem action)
        {
            if (sector.FDIndex == 0)
            {
                return;
            }

            List<FDEntry> entries = control.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry);
            foreach (FDEntry entry in entries)
            {
                (entry as FDTriggerEntry).TrigActionList.RemoveAll(a => a.TrigAction == action.TrigAction && a.Parameter == action.Parameter);
            }
        }
    }
}