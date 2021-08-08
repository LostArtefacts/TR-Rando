using System;
using System.Collections.Generic;
using TR2RandomizerCore.Helpers;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class EnvironmentRandomizer : RandomizerBase
    {
        public bool EnforcedModeOnly { get; set;  }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

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
            if (mapping == null)
            {
                return;
            }

            // Process enforced packs first
            mapping.EnforcedSet.ApplyToLevel(level.Data);

            if (/*!EnforcedModeOnly && */mapping.RandomizedSet.Count > 0)
            {
                // Pick a random number of packs to apply, but at least 1
                int packCount = _generator.Next(1, mapping.RandomizedSet.Count + 1);
                List<EMEditorSet> randomSet = mapping.RandomizedSet.RandomSelection(_generator, packCount);
                foreach (EMEditorSet mod in randomSet)
                {
                    mod.ApplyToLevel(level.Data);
                }
            }
        }
    }
}