using TRDataControl.Environment;
using TRGE.Core;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

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
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath($@"TR3\Environment\{level.Name}-Environment.json"));
        mapping?.SetRemastered(true);
        return mapping;
    }

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
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        mapping.All.ApplyToLevel(level.Data, picker.Options);
    }

    private void FinalizeEnvironment(TR3RCombinedLevel level)
    {
        EMEditorMapping mapping = GetMapping(level);
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        if (mapping != null)
        {
            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }
        }

        TR3EnemyUtilities.CheckMonkeyPickups(level.Data, true);
    }
}
