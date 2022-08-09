using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorSet : List<BaseEMFunction>, ITextureModifier
    {
        // A set of modifications that must be done together e.g. adding a ladder and a step

        public void ApplyToLevel(TRLevel level, IEnumerable<EMType> excludedTypes = null)
        {
            if (IsApplicable(excludedTypes))
            {
                foreach (BaseEMFunction mod in this)
                {
                    mod.ApplyToLevel(level);
                }
            }
        }

        public void ApplyToLevel(TR2Level level, IEnumerable<EMType> excludedTypes = null)
        {
            if (IsApplicable(excludedTypes))
            {
                foreach (BaseEMFunction mod in this)
                {
                    mod.ApplyToLevel(level);
                }
            }
        }

        public void ApplyToLevel(TR3Level level, IEnumerable<EMType> excludedTypes = null)
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
            if (excludedTypes != null)
            {
                // The modification will only be performed if all types in this set are to be included.
                foreach (BaseEMFunction mod in this)
                {
                    if (excludedTypes.Contains(mod.EMType))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            // Find every EMTextureMap (or any other texture type) and remap the old texture indices to new.
            foreach (BaseEMFunction mod in this)
            {
                if (mod is ITextureModifier textureMod)
                {
                    textureMod.RemapTextures(indexMap);
                }
            }
        }
    }
}