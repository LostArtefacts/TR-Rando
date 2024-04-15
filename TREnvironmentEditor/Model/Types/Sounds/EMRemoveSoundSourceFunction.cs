using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMRemoveSoundSourceFunction : BaseEMFunction
{
    public TRSoundSource Source { get; set; }

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

    private bool RemoveSource(List<TRSoundSource> sources)
    {
        TRSoundSource source = sources.Find(s => s.X == Source.X && s.Y == Source.Y && s.Z == Source.Z);
        return source != null && sources.Remove(source);
    }
}
