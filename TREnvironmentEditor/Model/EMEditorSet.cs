using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorSet : List<BaseEMFunction>
    {
        // A set of modifications that must be done together e.g. adding a ladder and a step

        public void ApplyToLevel(TR2Level level, IEnumerable<EMType> excludedTypes)
        {
            if (IsApplicable(excludedTypes))
            {
                foreach (BaseEMFunction mod in this)
                {
                    mod.ApplyToLevel(level);
                }
            }
        }

        public bool IsApplicable(IEnumerable<EMType> excludedTypes)
        {
            // The modification will only be performed if all types in this set are to be included.
            foreach (BaseEMFunction mod in this)
            {
                if (excludedTypes.Contains(mod.EMType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}