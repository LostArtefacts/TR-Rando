using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model
{
    public class EMEditorGroupedSet : ITextureModifier
    {
        public EMEditorSet Leader { get; set; }
        public List<EMEditorSet> Followers { get; set; }

        public void ApplyToLevel(TRLevel level, EMEditorSet follower, EMOptions options)
        {
            if (Leader.IsApplicable(options) && follower.IsApplicable(options))
            {
                Leader.ApplyToLevel(level, options);
                follower.ApplyToLevel(level, options);
            }
        }

        public void ApplyToLevel(TR2Level level, EMEditorSet follower, EMOptions options)
        {
            if (Leader.IsApplicable(options) && follower.IsApplicable(options))
            {
                Leader.ApplyToLevel(level, options);
                follower.ApplyToLevel(level, options);
            }
        }

        public void ApplyToLevel(TR3Level level, EMEditorSet follower, EMOptions options)
        {
            if (Leader.IsApplicable(options) && follower.IsApplicable(options))
            {
                Leader.ApplyToLevel(level, options);
                follower.ApplyToLevel(level, options);
            }
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            Leader.RemapTextures(indexMap);
            Followers.ForEach(s => s.RemapTextures(indexMap));
        }
    }
}