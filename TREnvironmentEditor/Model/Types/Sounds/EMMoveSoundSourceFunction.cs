using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveSoundSourceFunction : BaseEMFunction
    {
        public int Index { get; set; }
        public EMLocation Relocation { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            TRSoundSource source = level.SoundSources[Index];
            source.X += Relocation.X;
            source.Y += Relocation.Y;
            source.Z += Relocation.Z;
        }
    }
}