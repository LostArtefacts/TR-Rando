using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XSFXTask : ITR2ProcessorTask
{
    private readonly List<TR2SFXDefinition> _soundEffects;

    public TR2XSFXTask()
    {
        _soundEffects = JsonUtils.ReadFile<List<TR2SFXDefinition>>("Resources/TR2/Audio/sfx.json");
    }

    public void Run(TR2CombinedLevel level)
    {
        if (!level.IsExpansion)
        {
            return;
        }

        foreach (var (sfxId, effect) in level.Data.SoundEffects)
        {
            var definition = _soundEffects.FirstOrDefault(e => 
                e.InternalIndex == sfxId && e.GoldSampleRemap.HasValue);
            if (definition != null)
            {
                effect.SampleID = definition.GoldSampleRemap.Value;
            }
        }
    }
}
