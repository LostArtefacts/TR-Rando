using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertTriggerFunction : BaseEMFunction
    {
        public EMLocation Location { get; set; }
        public List<EMLocation> Locations { get; set; }
        public FDTrigType? TrigType { get; set; }
        public bool? OneShot { get; set; }
        public ushort? SwitchOrKeyRef { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            InitialiseLocations();

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ConvertTrigger(sector, control);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            InitialiseLocations();

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ConvertTrigger(sector, control);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            InitialiseLocations();

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ConvertTrigger(sector, control);
            }

            control.WriteToLevel(level);
        }

        private void InitialiseLocations()
        {
            if (Locations == null)
            {
                Locations = new List<EMLocation>();
            }
            if (Location != null)
            {
                // For backwards compatibility with mods already defined - using the List is the preferred way
                Locations.Add(Location);
            }
        }

        private void ConvertTrigger(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex != 0)
            {
                IEnumerable<FDTriggerEntry> triggers = floorData.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry).Cast<FDTriggerEntry>();
                foreach (FDTriggerEntry trigger in triggers)
                {
                    if (TrigType.HasValue)
                    {
                        if (trigger.TrigType == FDTrigType.Pickup && TrigType.Value != FDTrigType.Pickup && trigger.TrigActionList.Count > 0)
                        {
                            // The first action entry for pickup triggers is the pickup reference itself, so
                            // this is no longer needed.
                            trigger.TrigActionList.RemoveAt(0);
                        }
                        trigger.TrigType = TrigType.Value;
                    }
                    if (OneShot.HasValue)
                    {
                        trigger.TrigSetup.OneShot = OneShot.Value;
                    }
                    if (SwitchOrKeyRef.HasValue)
                    {
                        trigger.SwitchOrKeyRef = SwitchOrKeyRef.Value;
                    }
                }
            }
        }
    }
}