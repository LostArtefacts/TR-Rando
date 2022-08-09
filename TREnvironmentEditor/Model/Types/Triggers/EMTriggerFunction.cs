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
        public List<short> Rooms { get; set; }
        public EMLocationExpander ExpandedLocations { get; set; }
        public EMTrigger Trigger { get; set; }
        public bool Replace { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (Locations != null)
            {
                foreach (EMLocation location in Locations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    CreateTrigger(sector, control, triggerEntry);
                }
            }

            if (Rooms != null)
            {
                foreach (short room in Rooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                    {
                        if (!sector.IsImpenetrable && sector.RoomBelow == 255)
                        {
                            CreateTrigger(sector, control, triggerEntry);
                        }
                    }
                }
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (Locations != null)
            {
                foreach (EMLocation location in Locations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    CreateTrigger(sector, control, triggerEntry);
                }
            }

            if (Rooms != null)
            {
                foreach (short room in Rooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].SectorList)
                    {
                        if (!sector.IsImpenetrable && sector.RoomBelow == 255)
                        {
                            CreateTrigger(sector, control, triggerEntry);
                        }
                    }
                }
            }

            // Handle any specifics that the trigger may rely on
            foreach (FDActionListItem action in triggerEntry.TrigActionList)
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
            EMLevelData data = GetData(level);
            FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (Locations != null)
            {
                foreach (EMLocation location in Locations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    CreateTrigger(sector, control, triggerEntry);
                }
            }

            if (Rooms != null)
            {
                foreach (short room in Rooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                    {
                        if (!sector.IsImpenetrable && sector.RoomBelow == 255)
                        {
                            CreateTrigger(sector, control, triggerEntry);
                        }
                    }
                }
            }

            // Handle any specifics that the trigger may rely on
            foreach (FDActionListItem action in triggerEntry.TrigActionList)
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

        private FDTriggerEntry InitialiseTriggerEntry(EMLevelData data)
        {
            // Expand locations
            if (ExpandedLocations != null)
            {
                Locations = ExpandedLocations.Expand();
            }
            
            return Trigger.ToFDEntry(data);
        }

        private void CreateTrigger(TRRoomSector sector, FDControl control, FDTriggerEntry triggerEntry)
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
                entries.Add(triggerEntry);
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