using System.Collections.Generic;
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
        public List<EMLocation> Locations { get; set; }
        public EMLocationExpander LocationExpander { get; set; }
        public List<EMTriggerAction> Actions { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);
            List<EMLocation> locations = InitialiseLocations();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            bool appended = false;
            foreach (EMLocation location in locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, floorData);
                appended = AppendActions(sector, floorData, actions);
            }

            if (appended)
            {
                floorData.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);
            List<EMLocation> locations = InitialiseLocations();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            bool appended = false;
            foreach (EMLocation location in locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, floorData);
                appended = AppendActions(sector, floorData, actions);
            }

            if (appended)
            {
                floorData.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            List<FDActionListItem> actions = InitialiseActionItems(data);
            List<EMLocation> locations = InitialiseLocations();

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            bool appended = false;
            foreach (EMLocation location in locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, floorData);
                appended = AppendActions(sector, floorData, actions);
            }

            if (appended)
            {
                floorData.WriteToLevel(level);
            }
        }

        private List<EMLocation> InitialiseLocations()
        {
            List<EMLocation> locations = new List<EMLocation>();
            if (Location != null)
            {
                locations.Add(Location);
            }
            if (Locations != null)
            {
                locations.AddRange(Locations);
            }
            if (LocationExpander != null)
            {
                locations.AddRange(LocationExpander.Expand());
            }

            return locations;
        }

        private List<FDActionListItem> InitialiseActionItems(EMLevelData data)
        {
            List<FDActionListItem> actions = new List<FDActionListItem>();
            foreach (EMTriggerAction action in Actions)
            {
                actions.Add(action.ToFDAction(data));
            }
            return actions;
        }

        private bool AppendActions(TRRoomSector sector, FDControl floorData, List<FDActionListItem> actions)
        {
            if (sector.FDIndex != 0 && floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger)
            {
                trigger.TrigActionList.AddRange(actions);
                return true;
            }

            return false;
        }
    }
}