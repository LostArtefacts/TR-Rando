using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMConditionalGroupedSet : ITextureModifier
    {
        public BaseEMCondition Condition { get; set; }
        public EMEditorGroupedSet OnTrue { get; set; }
        public EMEditorGroupedSet OnFalse { get; set; }

        public EMEditorGroupedSet GetApplicableSet(TRLevel level)
        {
            return Condition.GetResult(level) ? OnTrue : OnFalse;
        }

        public EMEditorGroupedSet GetApplicableSet(TR2Level level)
        {
            return Condition.GetResult(level) ? OnTrue : OnFalse;
        }

        public EMEditorGroupedSet GetApplicableSet(TR3Level level)
        {
            return Condition.GetResult(level) ? OnTrue : OnFalse;
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            if (OnTrue != null)
            {
                OnTrue.RemapTextures(indexMap);
            }
            if (OnFalse != null)
            {
                OnFalse.RemapTextures(indexMap);
            }
        }
    }
}
