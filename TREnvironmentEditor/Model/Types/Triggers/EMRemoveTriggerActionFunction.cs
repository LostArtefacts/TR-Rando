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
        public FDActionListItem Action { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)ConvertItemNumber(location.Room, level.NumRooms), level, control);
                RemoveAction(baseSector, control);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)ConvertItemNumber(location.Room, level.NumRooms), level, control);
                RemoveAction(baseSector, control);
            }

            control.WriteToLevel(level);
        }

        private void RemoveAction(TRRoomSector sector, FDControl control)
        {
            if (sector.FDIndex == 0)
            {
                return;
            }

            List<FDEntry> entries = control.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry);
            foreach (FDEntry entry in entries)
            {
                (entry as FDTriggerEntry).TrigActionList.RemoveAll(a => a.TrigAction == Action.TrigAction && a.Parameter == Action.Parameter);
            }
        }
    }
}