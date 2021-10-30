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
        public FDTrigAction TrigAction { get; set; }
        public ushort NewParameter { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
                ReplaceActionParameter(baseSector, control);
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
                ReplaceActionParameter(baseSector, control);
            }

            control.WriteToLevel(level);
        }

        private void ReplaceActionParameter(TRRoomSector baseSector, FDControl control)
        {
            if (baseSector.FDIndex == 0)
            {
                return;
            }

            List<FDActionListItem> actions = FDUtilities.GetActionListItems(control, TrigAction, baseSector.FDIndex);
            foreach (FDActionListItem action in actions)
            {
                action.Parameter = NewParameter;
            }
        }
    }
}