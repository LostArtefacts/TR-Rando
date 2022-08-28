using System;
using System.Collections.Generic;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers
{
    public class TR1SecretRewardRandomizer : BaseTR1Randomizer
    {
        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            foreach (TR1ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeRewards(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeRewards(TR1CombinedLevel level)
        {
            if (level.IsAssault)
            {
                return;
            }

            TRSecretMapping<TREntity> secretMapping = TRSecretMapping<TREntity>.Get(GetResourcePath(@"TR1\SecretMapping\" + level.Name + "-SecretMapping.json"));

            List<TREntities> stdItemTypes = TR1EntityUtilities.GetStandardPickupTypes();
            stdItemTypes.Remove(TREntities.PistolAmmo_S_P); // Sprite/model not available
            stdItemTypes.Remove(TREntities.Pistols_S_P); // A bit cruel as a reward?

            for (int i = 0; i < level.Data.NumEntities; i++)
            {
                if (!secretMapping.RewardEntities.Contains(i))
                {
                    continue;
                }

                level.Data.Entities[i].TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
            }
        }
    }
}