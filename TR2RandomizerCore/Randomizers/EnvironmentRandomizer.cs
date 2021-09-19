using System;
using System.Collections.Generic;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Utilities;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelReader.Helpers;

namespace TR2RandomizerCore.Randomizers
{
    public class EnvironmentRandomizer : RandomizerBase
    {
        public bool EnforcedModeOnly { get; set; }
        public uint NumMirrorLevels { get; set; }
        public bool MirrorAssaultCourse { get; set; }
        public bool RandomizeWater { get; set; }
        public bool RandomizeSlots { get; set; }
        public bool RandomizeLadders { get; set; }

        internal TexturePositionMonitorBroker TextureMonitor { get; set; }

        private List<EMType> _disallowedTypes;
        private List<TR23ScriptedLevel> _levelsToMirror;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            _disallowedTypes = new List<EMType>();
            if (!RandomizeWater)
            {
                _disallowedTypes.Add(EMType.Flood);
                _disallowedTypes.Add(EMType.Drain);
            }
            if (!RandomizeSlots)
            {
                _disallowedTypes.Add(EMType.MoveSlot);
                _disallowedTypes.Add(EMType.SwapSlot);
            }
            if (!RandomizeLadders)
            {
                _disallowedTypes.Add(EMType.Ladder);
            }

            _levelsToMirror = Levels.RandomSelection(_generator, (int)NumMirrorLevels, exclusions:new HashSet<TR23ScriptedLevel>
            {
                Levels.Find(l => l.Is(LevelNames.ASSAULT))
            });

            foreach (TR23ScriptedLevel lvl in Levels)
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
            EMEditorMapping mapping = EMEditorMapping.Get(level.Name);
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

            if (!EnforcedModeOnly && (_levelsToMirror.Contains(level.Script) || (level.IsAssault && MirrorAssaultCourse)))
            {
                MirrorLevel(level, mapping);
            }
        }

        private void ApplyMappingToLevel(TR2CombinedLevel level, EMEditorMapping mapping)
        {
            // Process enforced packs first. We do not pass disallowed types here.
            mapping.All.ApplyToLevel(level.Data, new EMType[] { });

            if (EnforcedModeOnly)
            {
                return;
            }

            if (mapping.Any.Count > 0)
            {
                // Pick a random number of packs to apply, but at least 1
                int packCount = _generator.Next(1, mapping.Any.Count + 1);
                List<EMEditorSet> randomSet = mapping.Any.RandomSelection(_generator, packCount);
                foreach (EMEditorSet mod in randomSet)
                {
                    mod.ApplyToLevel(level.Data, _disallowedTypes);
                }
            }

            // AllWithin means one from each set will be applied. Used for the likes of choosing a new
            // keyhole position from a set.
            if (mapping.AllWithin.Count > 0)
            {
                foreach (List<EMEditorSet> modList in mapping.AllWithin)
                {
                    EMEditorSet mod = modList[_generator.Next(0, modList.Count)];
                    mod.ApplyToLevel(level.Data, _disallowedTypes);
                }
            }

            // OneOf is used for a leader-follower situation, but where only one follower from
            // a group is wanted. An example is removing a ladder (the leader) and putting it in 
            // a different position, so the followers are the different positions from which we pick one.
            if (mapping.OneOf.Count > 0)
            {
                foreach (EMEditorGroupedSet mod in mapping.OneOf)
                {
                    EMEditorSet follower = mod.Followers[_generator.Next(0, mod.Followers.Count)];
                    mod.ApplyToLevel(level.Data, follower, _disallowedTypes);
                }
            }
        }

        private void MirrorLevel(TR2CombinedLevel level, EMEditorMapping mapping)
        {
            EMMirrorFunction mirrorer = new EMMirrorFunction();
            mirrorer.ApplyToLevel(level.Data);

            if (mapping != null)
            {
                // Process packs that need to be applied after mirroring.
                mapping.Mirrored.ApplyToLevel(level.Data, new EMType[] { });
            }

            // Notify the texture monitor that this level has been flipped
            TexturePositionMonitor monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }
    }
}