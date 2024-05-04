using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRemoveSoundSourceFunction : BaseEMFunction
{
    public TRSoundSource<TR1SFX> Source { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        RemoveSource(level.SoundSources);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        RemoveSource(level.SoundSources);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        RemoveSource(level.SoundSources);
    }

    private bool RemoveSource<T>(List<TRSoundSource<T>> sources)
        where T : Enum
    {
        TRSoundSource<T> source = sources.Find(s => s.X == Source.X && s.Y == Source.Y && s.Z == Source.Z);
        return source != null && sources.Remove(source);
    }
}
