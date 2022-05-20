using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMDuplicateSwitchTriggerFunction : EMDuplicateTriggerFunction
    {
        public ushort NewSwitchIndex { get; set; }
        public ushort OldSwitchIndex { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            // Get a location for the switch we're interested in
            TR2Entity switchEntity = level.Entities[NewSwitchIndex];
            Locations = new List<EMLocation>
            {
                new EMLocation
                {
                    X = switchEntity.X,
                    Y = switchEntity.Y,
                    Z = switchEntity.Z,
                    Room = data.ConvertRoom(switchEntity.Room)
                }
            };

            // Get the location of the old switch
            switchEntity = level.Entities[OldSwitchIndex];
            BaseLocation = new EMLocation
            {
                X = switchEntity.X,
                Y = switchEntity.Y,
                Z = switchEntity.Z,
                Room = data.ConvertRoom(switchEntity.Room)
            };

            // Duplicate the triggers to the switch's location
            base.ApplyToLevel(level);

            // Go one step further and replace the duplicated trigger with the new switch ref
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);

                List<FDEntry> keyTriggers = control.Entries[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
                foreach (FDEntry entry in keyTriggers)
                {
                    (entry as FDTriggerEntry).SwitchOrKeyRef = NewSwitchIndex;
                }
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            throw new System.NotImplementedException();
        }
    }
}