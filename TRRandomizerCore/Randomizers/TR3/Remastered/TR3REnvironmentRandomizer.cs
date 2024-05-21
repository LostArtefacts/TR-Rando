using TRDataControl.Environment;
using TRGE.Core;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR3REnvironmentRandomizer : BaseTR3RRandomizer
{
    public override void Randomize(int seed)
    {
        _generator ??= new(seed);

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            RandomizeEnvironment(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void FinalizeEnvironment()
    {
        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            FinalizeEnvironment(_levelInstance);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private EMEditorMapping GetMapping(TR3RCombinedLevel level)
        => EMEditorMapping.Get(GetResourcePath($@"TR3\Environment\{level.Name}-Environment.json"));

    private void RandomizeEnvironment(TR3RCombinedLevel level)
    {
        EMEditorMapping mapping = GetMapping(level);
        if (mapping != null)
        {
            ApplyMappingToLevel(level, mapping);
        }
    }

    private void ApplyMappingToLevel(TR3RCombinedLevel level, EMEditorMapping mapping)
    {
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode)
        {
            Generator = _generator
        };
        picker.LoadTags(Settings, true);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        mapping.All.ApplyToLevel(level.Data, picker.Options);

        // No further mods supported yet
    }

    private void FinalizeEnvironment(TR3RCombinedLevel level)
    {
        EMEditorMapping mapping = GetMapping(level);
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;
        picker.ResetTags(true);

        if (mapping != null)
        {
            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }
        }
    }
}
