using System;
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

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            SetupLocations(data, level.Entities);

            // Duplicate the triggers to the switch's location
            base.ApplyToLevel(level);

            // Go one step further and replace the duplicated trigger with the new switch ref
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            UpdateTriggers(data, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            SetupLocations(data, level.Entities);

            // Duplicate the triggers to the switch's location
            base.ApplyToLevel(level);

            // Go one step further and replace the duplicated trigger with the new switch ref
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            UpdateTriggers(data, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            SetupLocations(data, level.Entities);

            base.ApplyToLevel(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            UpdateTriggers(data, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        private void SetupLocations(EMLevelData data, TREntity[] entities)
        {
            // Get a location for the switch we're interested in
            TREntity switchEntity = entities[data.ConvertEntity(NewSwitchIndex)];
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
            switchEntity = entities[data.ConvertEntity(OldSwitchIndex)];
            BaseLocation = new EMLocation
            {
                X = switchEntity.X,
                Y = switchEntity.Y,
                Z = switchEntity.Z,
                Room = data.ConvertRoom(switchEntity.Room)
            };
        }

        private void SetupLocations(EMLevelData data, TR2Entity[] entities)
        {
            // Get a location for the switch we're interested in
            TR2Entity switchEntity = entities[data.ConvertEntity(NewSwitchIndex)];
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
            switchEntity = entities[data.ConvertEntity(OldSwitchIndex)];
            BaseLocation = new EMLocation
            {
                X = switchEntity.X,
                Y = switchEntity.Y,
                Z = switchEntity.Z,
                Room = data.ConvertRoom(switchEntity.Room)
            };
        }

        private void UpdateTriggers(EMLevelData data, FDControl control, Func<EMLocation, TRRoomSector> sectorGetter)
        {
            ushort newSwitchIndex = (ushort)data.ConvertEntity(NewSwitchIndex);
            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = sectorGetter.Invoke(location);

                List<FDEntry> keyTriggers = control.Entries[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
                foreach (FDEntry entry in keyTriggers)
                {
                    (entry as FDTriggerEntry).SwitchOrKeyRef = newSwitchIndex;
                }
            }
        }
    }
}