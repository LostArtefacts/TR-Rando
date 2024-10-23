using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers;

public class TR3RSecretRewardRandomizer : BaseTR3RRandomizer
{
    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR3SecretRewardAllocator allocator = new()
        {
            Generator = _generator
        };

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            TRSecretMapping<TR3Entity> mapping = TRSecretMapping<TR3Entity>.Get($"Resources/TR3/SecretMapping/{_levelInstance.Name}-SecretMapping.json");
            if (mapping != null)
            {
                allocator.RandomizeRewards(_levelInstance.Data, mapping.RewardEntities);
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
