using TREnvironmentEditor;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR2EnvironmentRandomizer : BaseTR2Randomizer, IMirrorControl
{
    internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
    internal TR2TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR2ScriptedLevel> _levelsToMirror;

    public void AllocateMirroredLevels(int seed)
    {
        if (!Settings.RandomizeEnvironment || _levelsToMirror != null)
        {
            return;
        }

        // This will only allocate once
        _generator ??= new Random(seed);

        TR2ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
        _levelsToMirror = Levels.RandomSelection(_generator, (int)Settings.MirroredLevelCount, exclusions: new HashSet<TR2ScriptedLevel>
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
        TR2ScriptedLevel level = Levels.Find(l => l.Is(levelName));
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
        _generator = new Random(seed);

        AllocateMirroredLevels(seed);

        foreach (TR2ScriptedLevel lvl in Levels)
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

    private void RandomizeEnvironment(TR2CombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath(@"TR2\Environment\" + level.Name + "-Environment.json"));
        if (mapping != null)
        {
            if (level.IsUKBox)
            {
                // The mapping is configured for EPC and Multipatch texture indices, but should
                // have alternate mapping defined for UKBox, so switch to it in this case.
                mapping.AlternateTextures();
            }
            ApplyMappingToLevel(level, mapping);
        }

        if (_levelsToMirror?.Contains(level.Script) ?? false)
        {
            MirrorLevel(level, mapping);
        }
    }

    private void ApplyMappingToLevel(TR2CombinedLevel level, EMEditorMapping mapping)
    {
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode)
        {
            Generator = _generator
        };
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        // Process enforced packs first. We do not pass disallowed types here.
        // These generally fix OG issues such as problems with box overlaps and
        // textures.
        mapping.All.ApplyToLevel(level.Data, picker.Options);
        
        if (!EnforcedModeOnly || !Settings.PuristMode)
        {
            // Non-purist packs generally make return paths available.
            // These are applied only if Purist mode is off or if Environment
            // rando is on as a whole, because some other categories may rely
            // on these changes having been made.
            mapping.NonPurist.ApplyToLevel(level.Data, picker.Options);
        }

        if (!EnforcedModeOnly)
        {
            picker.LoadTags(Settings, ScriptEditor.Edition.IsCommunityPatch);
            picker.Options.ExclusionMode = EMExclusionMode.BreakOnAny;

            // Run a random selection of Any.
            foreach (EMEditorSet mod in picker.GetRandomAny(mapping))
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }

            // AllWithin means one from each set will be applied. Used for the likes of choosing a new
            // keyhole position from a set.
            foreach (List<EMEditorSet> modList in mapping.AllWithin)
            {
                picker.GetModToRun(modList)?.ApplyToLevel(level.Data, picker.Options);
            }

            // OneOf is used for a leader-follower situation, but where only one follower from
            // a group is wanted. An example is removing a ladder (the leader) and putting it in 
            // a different position, so the followers are the different positions from which we pick one.
            foreach (EMEditorGroupedSet mod in mapping.OneOf)
            {
                if (picker.GetModToRun(mod.Followers) is EMEditorSet follower)
                {
                    mod.ApplyToLevel(level.Data, follower, picker.Options);
                }
            }

            // ConditionalAllWithin is similar to AllWithin, but different sets of mods can be returned based
            // on a given condition. For example, move a slot to a room, but only if a specific entity exists.
            foreach (EMConditionalEditorSet conditionalSet in mapping.ConditionalAllWithin)
            {
                List<EMEditorSet> modList = conditionalSet.GetApplicableSets(level.Data);
                if (modList != null && modList.Count > 0)
                {
                    picker.GetModToRun(modList)?.ApplyToLevel(level.Data, picker.Options);
                }
            }

            // Identical to OneOf but different sets can be returned based on a given condition.
            foreach (EMConditionalGroupedSet conditionalSet in mapping.ConditionalOneOf)
            {
                EMEditorGroupedSet mod = conditionalSet.GetApplicableSet(level.Data);
                if (mod != null && picker.GetModToRun(mod.Followers) is EMEditorSet follower)
                {
                    mod.ApplyToLevel(level.Data, follower, picker.Options);
                }
            }
        }

        // Similar to All, but these mods will have conditions configured so may
        // or may not apply. Process these last so that conditions based on other
        // mods can be used.
        picker.Options.ExclusionMode = EMExclusionMode.Individual;
        picker.ResetTags(ScriptEditor.Edition.IsCommunityPatch);
        foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
        {
            mod.ApplyToLevel(level.Data, picker.Options);
        }
    }

    private void MirrorLevel(TR2CombinedLevel level, EMEditorMapping mapping)
    {
        EMMirrorFunction mirrorer = new();
        mirrorer.ApplyToLevel(level.Data);

        EnvironmentPicker picker = new(Settings.HardEnvironmentMode);
        picker.LoadTags(Settings, ScriptEditor.Edition.IsCommunityPatch);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;
        mapping?.Mirrored.ApplyToLevel(level.Data, picker.Options);

        // Notify the texture monitor that this level has been flipped
        TextureMonitor<TR2Entities> monitor = TextureMonitor.CreateMonitor(level.Name);
        monitor.UseMirroring = true;
    }
}
