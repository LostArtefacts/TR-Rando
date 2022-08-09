using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMModelExistsCondition : BaseEMCondition
    {
        public uint ModelID { get; set; }

        protected override bool Evaluate(TRLevel level)
        {
            TRModel model = level.Models.ToList().Find(m => m.ID == ModelID);
            return model != null;
        }

        protected override bool Evaluate(TR2Level level)
        {
            TRModel model = level.Models.ToList().Find(m => m.ID == ModelID);
            return model != null;
        }

        protected override bool Evaluate(TR3Level level)
        {
            TRModel model = level.Models.ToList().Find(m => m.ID == ModelID);
            return model != null;
        }
    }
}