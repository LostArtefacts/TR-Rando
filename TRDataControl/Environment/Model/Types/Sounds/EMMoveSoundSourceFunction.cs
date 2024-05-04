using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMoveSoundSourceFunction : BaseEMFunction
{
    public int Index { get; set; }
    public EMLocation Relocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
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

    private void MoveSoundSource<T>(TRSoundSource<T> source)
        where T : Enum
    {
        source.X += Relocation.X;
        source.Y += Relocation.Y;
        source.Z += Relocation.Z;
    }
}
