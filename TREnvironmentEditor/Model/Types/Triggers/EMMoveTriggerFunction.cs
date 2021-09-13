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

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, BaseLocation.Room, level, control);
            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level, control);

            if (baseSector.FDIndex != 0)
            {
                if (newSector.FDIndex == 0)
                {
                    control.CreateFloorData(newSector);
                }

                List<FDEntry> triggers = control.Entries[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
                control.Entries[newSector.FDIndex].AddRange(triggers);

                control.Entries[baseSector.FDIndex].RemoveAll(e => triggers.Contains(e));
                if (control.Entries[baseSector.FDIndex].Count == 0)
                {
                    control.RemoveFloorData(baseSector);
                }

                control.WriteToLevel(level);
            }
        }
    }
}