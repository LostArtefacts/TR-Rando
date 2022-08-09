using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddSoundSourceFunction : BaseEMFunction
    {
        public TRSoundSource Source { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            List<TRSoundSource> soundSources = level.SoundSources.ToList();
            soundSources.Add(Source);
            level.SoundSources = soundSources.ToArray();
            level.NumSoundSources++;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            List<TRSoundSource> soundSources = level.SoundSources.ToList();
            soundSources.Add(Source);
            level.SoundSources = soundSources.ToArray();
            level.NumSoundSources++;
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TRSoundSource> soundSources = level.SoundSources.ToList();
            soundSources.Add(Source);
            level.SoundSources = soundSources.ToArray();
            level.NumSoundSources++;
        }
    }
}