using System;
using System.Collections.Generic;
using TR2RandomizerCore.Environment;
using TR2RandomizerCore.Helpers;
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
            List<BaseEnvironmentModification> mods = EnvironmentModificationFactory.GetModifications(level.Name);
            foreach (BaseEnvironmentModification mod in mods)
            {
                if (!EnforcedModeOnly || mod.Enforced)
                {
                    mod.ApplyToLevel(level.Data);
                }
            }
        }
    }
}