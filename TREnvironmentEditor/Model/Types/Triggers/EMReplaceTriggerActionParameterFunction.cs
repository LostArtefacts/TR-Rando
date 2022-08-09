using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMReplaceTriggerActionParameterFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public EMTriggerAction Action { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            FDActionListItem action = InitialiseActionItem(data);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }

            control.WriteToLevel(level);
        }

        private FDActionListItem InitialiseActionItem(EMLevelData data)
        {
            return Action.ToFDAction(data);
        }

        private void ReplaceActionParameter(TRRoomSector baseSector, FDControl control, FDActionListItem actionItem)
        {
            if (baseSector.FDIndex == 0)
            {
                return;
            }

            List<FDActionListItem> actions = FDUtilities.GetActionListItems(control, actionItem.TrigAction, baseSector.FDIndex);
            foreach (FDActionListItem action in actions)
            {
                action.Parameter = actionItem.Parameter;
            }
        }
    }
}