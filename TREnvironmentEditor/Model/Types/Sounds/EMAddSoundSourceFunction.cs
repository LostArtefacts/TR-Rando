using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddSoundSourceFunction : BaseEMFunction
{
    public TRSoundSource Source { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        level.SoundSources.Add(Source);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        level.SoundSources.Add(Source);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        level.SoundSources.Add(Source);
    }
}
