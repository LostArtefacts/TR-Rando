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
        public FDActionListItem[] ActionItems { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)ConvertItemNumber(Location.Room, level.NumRooms), level, control);
            if (sector.FDIndex == 0)
            {
                return;
            }

            FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (trigger != null)
            {
                trigger.TrigActionList.AddRange(ActionItems);
                control.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)ConvertItemNumber(Location.Room, level.NumRooms), level, control);
            if (sector.FDIndex == 0)
            {
                return;
            }

            FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (trigger != null)
            {
                trigger.TrigActionList.AddRange(ActionItems);
                control.WriteToLevel(level);
            }
        }
    }
}