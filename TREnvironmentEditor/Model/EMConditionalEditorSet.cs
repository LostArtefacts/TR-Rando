using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMConditionalEditorSet
    {
        public BaseEMCondition Condition { get; set; }
        public List<EMEditorSet> OnTrue { get; set; }
        public List<EMEditorSet> OnFalse { get; set; }

        public List<EMEditorSet> GetApplicableSets(TR2Level level)
        {
            return Condition.GetResult(level) ? OnTrue : OnFalse;
        }
    }
}