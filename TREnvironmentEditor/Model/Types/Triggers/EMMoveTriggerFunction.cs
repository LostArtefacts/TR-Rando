using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveTriggerFunction : BaseEMFunction
    {
        public EMLocation BaseLocation { get; set; }
        public EMLocation NewLocation { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, (short)ConvertItemNumber(BaseLocation.Room, level.NumRooms), level, control);
            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, (short)ConvertItemNumber(NewLocation.Room, level.NumRooms), level, control);

            if (MoveTriggers(baseSector, newSector, control))
            {
                control.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, (short)ConvertItemNumber(BaseLocation.Room, level.NumRooms), level, control);
            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, (short)ConvertItemNumber(NewLocation.Room, level.NumRooms), level, control);

            if (MoveTriggers(baseSector, newSector, control))
            {
                control.WriteToLevel(level);
            }
        }

        private bool MoveTriggers(TRRoomSector baseSector, TRRoomSector newSector, FDControl control)
        {
            if (baseSector != newSector && baseSector.FDIndex != 0)
            {
                List<FDEntry> triggers = control.Entries[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
                if (triggers.Count > 0)
                {
                    if (newSector.FDIndex == 0)
                    {
                        control.CreateFloorData(newSector);
                    }

                    control.Entries[newSector.FDIndex].AddRange(triggers);

                    control.Entries[baseSector.FDIndex].RemoveAll(e => triggers.Contains(e));
                    if (control.Entries[baseSector.FDIndex].Count == 0)
                    {
                        control.RemoveFloorData(baseSector);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}