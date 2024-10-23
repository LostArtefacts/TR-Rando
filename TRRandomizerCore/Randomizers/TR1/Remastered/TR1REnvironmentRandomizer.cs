using TRDataControl.Environment;
using TRGE.Core;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR1REnvironmentRandomizer : BaseTR1RRandomizer
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

    private EMEditorMapping GetMapping(TR1RCombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath($"TR1/Environment/{level.Name}-Environment.json"));
        mapping?.SetRemastered(true);
        return mapping;
    }

    private void RandomizeEnvironment(TR1RCombinedLevel level)
    {
        EMEditorMapping mapping = GetMapping(level);
        if (mapping != null)
        {
            ApplyMappingToLevel(level, mapping);
        }
    }

    private void ApplyMappingToLevel(TR1RCombinedLevel level, EMEditorMapping mapping)
    {
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        mapping.All.ApplyToLevel(level.Data, picker.Options);
    }

    private void FinalizeEnvironment(TR1RCombinedLevel level)
    {
        EMEditorMapping mapping = GetMapping(level);
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;
        picker.ResetTags();

        if (mapping != null)
        {
            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }
        }
    }
}
