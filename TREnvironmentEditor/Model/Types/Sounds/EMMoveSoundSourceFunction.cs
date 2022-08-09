using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveSoundSourceFunction : BaseEMFunction
    {
        public int Index { get; set; }
        public EMLocation Relocation { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            MoveSoundSource(level.SoundSources[Index]);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            MoveSoundSource(level.SoundSources[Index]);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            MoveSoundSource(level.SoundSources[Index]);
        }

        private void MoveSoundSource(TRSoundSource source)
        {
            source.X += Relocation.X;
            source.Y += Relocation.Y;
            source.Z += Relocation.Z;
        }
    }
}