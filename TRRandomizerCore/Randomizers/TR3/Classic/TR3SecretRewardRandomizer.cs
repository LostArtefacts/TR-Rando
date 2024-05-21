using TRGE.Core;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRewardRandomizer : BaseTR3Randomizer
{
    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR3SecretRewardAllocator allocator = new()
        {
            Generator = _generator
        };

        foreach (TR3ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            TR3SecretMapping mapping = TR3SecretMapping.Get(_levelInstance);
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
