using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRemoveSoundSourceFunction : BaseEMFunction
    {
        public TRSoundSource Source { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            // Can't remove by index in case several are called in succession
            List<TRSoundSource> sources = level.SoundSources.ToList();
            if (RemoveSource(sources))
            {
                level.SoundSources = sources.ToArray();
                level.NumSoundSources--;
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            // Can't remove by index in case several are called in succession
            List<TRSoundSource> sources = level.SoundSources.ToList();
            if (RemoveSource(sources))
            {
                level.SoundSources = sources.ToArray();
                level.NumSoundSources--;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TRSoundSource> sources = level.SoundSources.ToList();
            if (RemoveSource(sources))
            {
                level.SoundSources = sources.ToArray();
                level.NumSoundSources--;
            }
        }

        private bool RemoveSource(List<TRSoundSource> sources)
        {
            TRSoundSource source = sources.Find(s => s.X == Source.X && s.Y == Source.Y && s.Z == Source.Z);
            if (source != null)
            {
                return sources.Remove(source);
            }
            return false;
        }
    }
}