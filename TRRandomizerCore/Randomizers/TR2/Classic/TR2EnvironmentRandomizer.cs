using TRDataControl.Environment;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR2EnvironmentRandomizer : BaseTR2Randomizer, IMirrorControl
{
    internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
    internal TR2TextureMonitorBroker TextureMonitor { get; set; }

    private List<TRXScriptedLevel> _levelsToMirror;

    public void AllocateMirroredLevels(int seed)
    {
        if (!Settings.RandomizeEnvironment || _levelsToMirror != null)
        {
            return;
        }

        _generator ??= new(seed);

        var assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
        _levelsToMirror = Levels.RandomSelection(_generator, (int)Settings.MirroredLevelCount, exclusions: new HashSet<TRXScriptedLevel>
        {
            assaultCourse
        });

        if (Settings.MirrorAssaultCourse)
        {
            _levelsToMirror.Add(assaultCourse);
        }
    }

    public bool IsMirrored(string levelName)
    {
        return _levelsToMirror?.Contains(Levels.Find(l => l.Is(levelName))) ?? false;
    }

    public void SetIsMirrored(string levelName, bool mirrored)
    {
        var level = Levels.Find(l => l.Is(levelName));
        if (level == null)
        {
            return;
        }

        _levelsToMirror ??= new();
        if (mirrored && !_levelsToMirror.Contains(level))
        {
            _levelsToMirror.Add(level);
        }
        else if (!mirrored)
        {
            _levelsToMirror.Remove(level);
        }
    }

    public override void Randomize(int seed)
    {
        _generator ??= new(seed);

        AllocateMirroredLevels(seed);

        foreach (var lvl in Levels)
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
        foreach (var lvl in Levels)
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

    private void RandomizeEnvironment(TR2CombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath("TR2/Environment/" + level.Name + "-Environment.json"));
        if (mapping != null)
        {
            mapping.SetCommunityPatch(ScriptEditor.Edition.IsCommunityPatch);
            if (level.IsUKBox)
            {
                // The mapping is configured for EPC and Multipatch texture indices, but should
                // have alternate mapping defined for UKBox, so switch to it in this case.
                mapping.AlternateTextures();
            }
            ApplyMappingToLevel(level, mapping);
        }
    }

    private void ApplyMappingToLevel(TR2CombinedLevel level, EMEditorMapping mapping)
    {
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        mapping.All.ApplyToLevel(level.Data, picker.Options);

        if (EnforcedModeOnly)
        {
            return;
        }

        picker.Options.ExclusionMode = EMExclusionMode.BreakOnAny;

        foreach (EMEditorSet mod in picker.GetRandomAny(mapping))
        {
            mod.ApplyToLevel(level.Data, picker.Options);
        }

        foreach (List<EMEditorSet> modList in mapping.AllWithin)
        {
            picker.GetModToRun(modList)?.ApplyToLevel(level.Data, picker.Options);
        }

        foreach (EMEditorGroupedSet mod in mapping.OneOf)
        {
            if (picker.GetModToRun(mod.Followers) is EMEditorSet follower)
            {
                mod.ApplyToLevel(level.Data, follower, picker.Options);
            }
        }

        foreach (EMConditionalEditorSet conditionalSet in mapping.ConditionalAllWithin)
        {
            List<EMEditorSet> modList = conditionalSet.GetApplicableSets(level.Data);
            if (modList != null && modList.Count > 0)
            {
                picker.GetModToRun(modList)?.ApplyToLevel(level.Data, picker.Options);
            }
        }

        foreach (EMConditionalGroupedSet conditionalSet in mapping.ConditionalOneOf)
        {
            EMEditorGroupedSet mod = conditionalSet.GetApplicableSet(level.Data);
            if (mod != null && picker.GetModToRun(mod.Followers) is EMEditorSet follower)
            {
                mod.ApplyToLevel(level.Data, follower, picker.Options);
            }
        }
    }

    private void FinalizeEnvironment(TR2CombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath($"TR2/Environment/{level.Name}-Environment.json"));
        EnvironmentPicker picker = new(_generator, Settings, ScriptEditor.Edition);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        if (mapping != null)
        {
            mapping.SetCommunityPatch(ScriptEditor.Edition.IsCommunityPatch);

            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }
        }

        if (_levelsToMirror?.Contains(level.Script) ?? false)
        {
            EMMirrorFunction mirrorer = new();
            mirrorer.ApplyToLevel(level.Data);

            mapping?.Mirrored.ApplyToLevel(level.Data, picker.Options);

            TextureMonitor<TR2Type> monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }
    }
}
