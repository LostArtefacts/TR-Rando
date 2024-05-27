using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TR1RSecretRewardRandomizer : BaseTR1RRandomizer
{
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR1SecretRewardAllocator allocator = new()
        {
            ItemFactory = ItemFactory,
            Settings = Settings,
            Generator = _generator
        };

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            allocator.RandomizeRewards(_levelInstance.Name, _levelInstance.Data);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
