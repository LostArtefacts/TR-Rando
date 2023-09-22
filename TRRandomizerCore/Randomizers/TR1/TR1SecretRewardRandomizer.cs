using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers;

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

        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath(@"TR1\SecretMapping\" + level.Name + "-SecretMapping.json"));

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P); // Sprite/model not available
        stdItemTypes.Remove(TR1Type.Pistols_S_P); // A bit cruel as a reward?

        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            if (!secretMapping.RewardEntities.Contains(i))
            {
                continue;
            }

            level.Data.Entities[i].TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
        }
    }
}
