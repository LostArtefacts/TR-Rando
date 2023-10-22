using TREnvironmentEditor;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Handlers;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR1EnvironmentRandomizer : BaseTR1Randomizer, IMirrorControl
{
    internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
    internal TR1TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR1ScriptedLevel> _levelsToMirror;

    public void AllocateMirroredLevels(int seed)
    {
        if (!Settings.RandomizeEnvironment || _levelsToMirror != null)
        {
            return;
        }

        // This will only allocate once
        _generator ??= new(seed);

        TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR1LevelNames.ASSAULT));
        _levelsToMirror = Levels.RandomSelection(_generator, (int)Settings.MirroredLevelCount, exclusions: new HashSet<TR1ScriptedLevel>
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
        TR1ScriptedLevel level = Levels.Find(l => l.Is(levelName));
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

        foreach (TR1ScriptedLevel lvl in Levels)
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

    private void RandomizeEnvironment(TR1CombinedLevel level)
    {
        EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath(@"TR1\Environment\" + level.Name + "-Environment.json"));
        if (mapping != null)
        {
            mapping.SetCommunityPatch(ScriptEditor.Edition.IsCommunityPatch);
            ApplyMappingToLevel(level, mapping);
        }

        if (_levelsToMirror?.Contains(level.Script) ?? false)
        {
            MirrorLevel(level, mapping);
        }

        UpdateDoppelgangerScript(level);
    }

    private void ApplyMappingToLevel(TR1CombinedLevel level, EMEditorMapping mapping)
    {
        if ((level.Is(TR1LevelNames.CAVES) || level.Is(TR1LevelNames.FOLLY)) && level.Data.SoundMap[65] == -1)
        {
            // Caves and Folly have the swinging blade model (unused) but its SFX are missing - import here in case
            // any mods want to make use of the model.
            TR1Level vilcabamba = new TR1LevelControl().Read(Path.Combine(BackupPath, TR1LevelNames.VILCABAMBA));
            SoundUtilities.ImportLevelSound(level.Data, vilcabamba, new short[] { 65 });
            SoundUtilities.ResortSoundIndices(level.Data);
        }

        EnvironmentPicker picker = new(Settings.HardEnvironmentMode)
        {
            Generator = _generator
        };
        picker.LoadTags(Settings, ScriptEditor.Edition.IsCommunityPatch);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        // These are applied whether or not environment randomization is enabled,
        // but tags can still be used to filter out based on user preferences.
        mapping.All.ApplyToLevel(level.Data, picker.Options);

        if (!EnforcedModeOnly)
        {
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

    private void MirrorLevel(TR1CombinedLevel level, EMEditorMapping mapping)
    {
        EMMirrorFunction mirrorer = new();
        mirrorer.ApplyToLevel(level.Data);

        // Process packs that need to be applied after mirroring.
        EnvironmentPicker picker = new(Settings.HardEnvironmentMode);
        picker.LoadTags(Settings, ScriptEditor.Edition.IsCommunityPatch);
        picker.Options.ExclusionMode = EMExclusionMode.Individual;

        mapping?.Mirrored.ApplyToLevel(level.Data, picker.Options);

        // Notify the texture monitor that this level has been flipped
        TextureMonitor<TR1Type> monitor = TextureMonitor.CreateMonitor(level.Name);
        monitor.UseMirroring = true;

        if (ScriptEditor.Edition.IsCommunityPatch)
        {
            // Remove the demo if it's set as it can crash the game
            level.Script.Demo = null;
        }
    }

    private void UpdateDoppelgangerScript(TR1CombinedLevel level)
    {
        if (!ScriptEditor.Edition.IsCommunityPatch)
        {
            return;
        }

        // Bacon Lara may have been added as a trap/puzzle, so we need to ensure the script knows
        // where to set her up. The mods will have stored this in a temporary flag as the entity
        // starting room may not necessarily be where her positioning should be calculated from.
        int anchorRoom = Array.FindIndex(level.Data.Rooms, r => (r.Flags & EMAddDoppelgangerFunction.AnchorRoomFlag) > 0);
        if (anchorRoom == -1 && level.Is(TR1LevelNames.ATLANTIS))
        {
            // Extra check for OG Atlantis to ensure the script is configured properly.
            TR1Entity baconLara = level.Data.Entities.Find(e => e.TypeID == TR1Type.Doppelganger);
            if (baconLara?.Room == 57)
            {
                anchorRoom = 10;
            }
        }

        if (anchorRoom == -1)
        {
            level.Script.RemoveSequence(LevelSequenceType.Setup_Bacon_Lara);
            return;
        }

        if (level.Script.Sequences.Find(s => s.Type == LevelSequenceType.Setup_Bacon_Lara) is SetupBaconLaraSequence sequence)
        {
            sequence.AnchorRoom = anchorRoom;
        }
        else
        {
            level.Script.AddSequenceBefore(LevelSequenceType.Loop_Game, new SetupBaconLaraSequence
            {
                Type = LevelSequenceType.Setup_Bacon_Lara,
                AnchorRoom = anchorRoom
            });
        }

        level.Data.Rooms[anchorRoom].Flags &= (short)~EMAddDoppelgangerFunction.AnchorRoomFlag;
    }
}
