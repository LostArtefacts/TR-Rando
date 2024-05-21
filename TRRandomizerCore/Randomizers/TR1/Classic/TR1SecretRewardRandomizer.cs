using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class TR1SecretRewardRandomizer : BaseTR1Randomizer
{
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR1SecretRewardAllocator allocator = new()
        {
            ItemFactory = ItemFactory,
            Generator = _generator
        };

        foreach (TR1ScriptedLevel lvl in Levels)
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
