using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMTriggerFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public FDTriggerEntry TriggerEntry { get; set; }
        public bool Replace { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)ConvertItemNumber(location.Room, level.NumRooms), level, control);
                CreateTrigger(sector, control);
            }

            // Handle any specifics that the trigger may rely on
            foreach (FDActionListItem action in TriggerEntry.TrigActionList)
            {
                switch (action.TrigAction)
                {
                    case FDTrigAction.ClearBodies:
                        SetEnemyClearBodies(level.Entities);
                        break;
                }
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)ConvertItemNumber(location.Room, level.NumRooms), level, control);
                CreateTrigger(sector, control);
            }

            // Handle any specifics that the trigger may rely on
            foreach (FDActionListItem action in TriggerEntry.TrigActionList)
            {
                switch (action.TrigAction)
                {
                    case FDTrigAction.ClearBodies:
                        SetEnemyClearBodies(level.Entities);
                        break;
                }
            }

            control.WriteToLevel(level);
        }

        private void CreateTrigger(TRRoomSector sector, FDControl control)
        {
            // If there is no floor data create the FD to begin with.
            if (sector.FDIndex == 0)
            {
                control.CreateFloorData(sector);
            }

            List<FDEntry> entries = control.Entries[sector.FDIndex];
            if (Replace)
            {
                entries.Clear();
            }
            if (entries.FindIndex(e => e is FDTriggerEntry) == -1)
            {
                entries.Add(TriggerEntry);
            }
        }

        private void SetEnemyClearBodies(IEnumerable<TR2Entity> levelEntities)
        {
            foreach (TR2Entity entity in levelEntities)
            {
                if (TR2EntityUtilities.IsEnemyType((TR2Entities)entity.TypeID))
                {
                    entity.ClearBody = true;
                }
            }
        }
    }
}