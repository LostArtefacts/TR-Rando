using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMSectorContainsSecretCondition : BaseEMCondition
    {
        public EMLocation Location { get; set; }

        protected override bool Evaluate(TR1Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            return Location.GetContainedSecretEntity(level, floorData) != -1;
        }

        protected override bool Evaluate(TR2Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            return Location.GetContainedSecretEntity(level, floorData) != -1;
        }

        protected override bool Evaluate(TR3Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            return Location.GetContainedSecretEntity(level, floorData) != -1;
        }
    }
}
