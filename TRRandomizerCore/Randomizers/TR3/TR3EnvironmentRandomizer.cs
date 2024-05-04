using TRDataControl.Environment;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR3EnvironmentRandomizer : BaseTR3Randomizer, IMirrorControl
{
    internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
    internal TR3TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR3ScriptedLevel> _levelsToMirror;

    public void AllocateMirroredLevels(int seed)
    {
        if (!Settings.RandomizeEnvironment || _levelsToMirror != null)
        {
            return;
        }

        _generator ??= new(seed);

        TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
        _levelsToMirror = Levels.RandomSelection(_generator, (int)Settings.MirroredLevelCount, exclusions: new HashSet<TR3ScriptedLevel>
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
        TR3ScriptedLevel level = Levels.Find(l => l.Is(levelName));
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

        foreach (TR3ScriptedLevel lvl in Levels)
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
        foreach (TR3ScriptedLevel lvl in Levels)
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

    private void RandomizeEnvironment(TR3CombinedLevel level)
    {
        string json = $@"TR3\Environment\{level.Name}-Environment.json";
        if (IsJPVersion)
        {
            string jpJson = $@"TR3\Environment\{level.Name}-JP-Environment.json";
            if (ResourceExists(jpJson))
            {
                json = jpJson;
            }
        }

        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath(json));
        if (mapping != null)
        {
            mapping.SetCommunityPatch(ScriptEditor.Edition.IsCommunityPatch);
            ApplyMappingToLevel(level, mapping);
        }
    }

    private void ApplyMappingToLevel(TR3CombinedLevel level, EMEditorMapping mapping)
    {
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode)
        {
            Generator = _generator
        };
        picker.LoadTags(Settings, ScriptEditor.Edition.IsCommunityPatch);
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

    private void FinalizeEnvironment(TR3CombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath($@"TR3\Environment\{level.Name}-Environment.json"));
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;
        picker.ResetTags(ScriptEditor.Edition.IsCommunityPatch);

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

            TextureMonitor<TR3Type> monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }

        CheckMonkeyPickups(level);
    }

    private static void CheckMonkeyPickups(TR3CombinedLevel level)
    {
        // Do a global check for monkeys that may be sitting on more than one pickup.
        // This has to happen after item, enemy and environment rando to account for
        // any shifted, converted and added items.
        List<TR3Entity> monkeys = level.Data.Entities.FindAll(e => e.TypeID == TR3Type.Monkey);
        foreach (TR3Entity monkey in monkeys)
        {
            List<TR3Entity> pickups = level.Data.Entities.FindAll(e =>
                    e.X == monkey.X &&
                    e.Y == monkey.Y &&
                    e.Z == monkey.Z &&
                    TR3TypeUtilities.IsAnyPickupType(e.TypeID)).ToList();

            if (pickups.Count == 1)
            {
                continue;
            }

            // Leave one item to drop, favouring key items. The others will be shifted
            // slightly so the monkey doesn't pick them up.
            pickups.Sort((e1, e2) => TR3TypeUtilities.IsKeyItemType(e1.TypeID) ? 1 : -1);
            for (int i = 0; i < pickups.Count - 1; i++)
            {
                ++pickups[i].X;
            }
        }
    }
}
