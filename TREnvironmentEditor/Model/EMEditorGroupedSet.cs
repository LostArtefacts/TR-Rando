using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorGroupedSet : ITextureModifier
    {
        public EMEditorSet Leader { get; set; }
        public List<EMEditorSet> Followers { get; set; }

        public void ApplyToLevel(TRLevel level, EMEditorSet follower, IEnumerable<EMType> excludedTypes)
        {
            if (Leader.IsApplicable(excludedTypes) && follower.IsApplicable(excludedTypes))
            {
                Leader.ApplyToLevel(level, excludedTypes);
                follower.ApplyToLevel(level, excludedTypes);
            }
        }

        public void ApplyToLevel(TR2Level level, EMEditorSet follower, IEnumerable<EMType> excludedTypes)
        {
            if (Leader.IsApplicable(excludedTypes) && follower.IsApplicable(excludedTypes))
            {
                Leader.ApplyToLevel(level, excludedTypes);
                follower.ApplyToLevel(level, excludedTypes);
            }
        }

        public void ApplyToLevel(TR3Level level, EMEditorSet follower, IEnumerable<EMType> excludedTypes)
        {
            if (Leader.IsApplicable(excludedTypes) && follower.IsApplicable(excludedTypes))
            {
                Leader.ApplyToLevel(level, excludedTypes);
                follower.ApplyToLevel(level, excludedTypes);
            }
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            Leader.RemapTextures(indexMap);
            Followers.ForEach(s => s.RemapTextures(indexMap));
        }
    }
}