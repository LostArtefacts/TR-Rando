using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRemoveTriggerFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public List<int> Rooms { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (Locations != null)
            {
                foreach (EMLocation location in Locations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
                    RemoveSectorTriggers(sector, control);
                }
            }

            if (Rooms != null)
            {
                foreach (int roomNumber in Rooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[roomNumber].SectorList)
                    {
                        RemoveSectorTriggers(sector, control);
                    }
                }
            }

            control.WriteToLevel(level);
        }

        private void RemoveSectorTriggers(TRRoomSector sector, FDControl control)
        {
            if (sector.FDIndex == 0)
            {
                return;
            }

            List<FDEntry> entries = control.Entries[sector.FDIndex];
            entries.RemoveAll(e => e is FDTriggerEntry);
            if (entries.Count == 0)
            {
                // If there isn't anything left, reset the sector to point to the dummy FD
                control.RemoveFloorData(sector);
            }
        }
    }
}