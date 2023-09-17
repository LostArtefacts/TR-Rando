using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRewardRandomizer : BaseTR3Randomizer
{
    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        foreach (TR3ScriptedLevel lvl in Levels)
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

    private void RandomizeRewards(TR3CombinedLevel level)
    {
        if (level.IsAssault)
        {
            return;
        }

        TRSecretMapping<TR2Entity> secretMapping = TRSecretMapping<TR2Entity>.Get(GetResourcePath(@"TR3\SecretMapping\" + level.Name + "-SecretMapping.json"));

        List<TR3Type> stdItemTypes = TR3EntityUtilities.GetStandardPickupTypes();
        // A bit cruel as rewards?
        stdItemTypes.Remove(TR3Type.PistolAmmo_P);
        stdItemTypes.Remove(TR3Type.Pistols_P);

        foreach (int rewardIndex in secretMapping.RewardEntities)
        {
            if (level.Data.Entities[rewardIndex].TypeID == (short)TR3Type.SaveCrystal_P)
            {
                continue;
            }

            level.Data.Entities[rewardIndex].TypeID = (short)stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
        }
    }
}
