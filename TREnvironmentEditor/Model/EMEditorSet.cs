using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorSet : List<BaseEMFunction>
    {
        // A set of modifications that must be done together e.g. adding a ladder and a step

        public void ApplyToLevel(TR2Level level)
        {
            foreach (BaseEMFunction mod in this)
            {
                mod.ApplyToLevel(level);
            }
        }
    }
}