using System;
using System.Collections.Generic;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR3EnvironmentRandomizer : BaseTR3Randomizer
    {
        internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
        internal TR3TextureMonitorBroker TextureMonitor { get; set; }

        private List<TR3ScriptedLevel> _levelsToMirror;

        public List<TR3ScriptedLevel> AllocateMirroredLevels(int seed)
        {
            if (!Settings.RandomizeEnvironment)
            {
                return new List<TR3ScriptedLevel>();
            }

            // This will only allocate once
            if (_generator == null)
            {
                _generator = new Random(seed);
            }

            if (_levelsToMirror == null)
            {
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

            return new List<TR3ScriptedLevel>(_levelsToMirror);
        }

        public override void Randomize(int seed)
        {
            if (_generator == null)
            {
                _generator = new Random(seed);
            }

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

        private void RandomizeEnvironment(TR3CombinedLevel level)
        {
            string json = @"TR3\Environment\" + level.Name + "-Environment.json";
            if (IsJPVersion)
            {
                string jpJson = @"TR3\Environment\" + level.Name + "-JP-Environment.json";
                if (ResourceExists(jpJson))
                {
                    json = jpJson;
                }
            }

            EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath(json));
            if (mapping != null)
            {
                ApplyMappingToLevel(level, mapping);
            }

            if (!EnforcedModeOnly && _levelsToMirror.Contains(level.Script))
            {
                MirrorLevel(level, mapping);
            }
        }

        private void ApplyMappingToLevel(TR3CombinedLevel level, EMEditorMapping mapping)
        {
            EnvironmentPicker picker = new EnvironmentPicker(Settings.HardEnvironmentMode)
            {
                Generator = _generator
            };

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
                picker.LoadTags(Settings);

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
            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, picker.Options);
            }
        }

        private void MirrorLevel(TR3CombinedLevel level, EMEditorMapping mapping)
        {
            EMMirrorFunction mirrorer = new EMMirrorFunction();
            mirrorer.ApplyToLevel(level.Data);

            EnvironmentPicker picker = new EnvironmentPicker(Settings.HardEnvironmentMode);
            picker.LoadTags(Settings);
            mapping?.Mirrored.ApplyToLevel(level.Data, picker.Options);

            // Notify the texture monitor that this level has been flipped
            TextureMonitor<TR3Entities> monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }
    }
}