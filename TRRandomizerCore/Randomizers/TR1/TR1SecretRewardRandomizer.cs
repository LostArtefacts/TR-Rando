using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1SecretRewardRandomizer : BaseTR1Randomizer
{
    private static readonly int _minRewardCount = 3;
    private static readonly double _doubleRewardChance = (double)1 / 6;
    private static readonly double _extraRewardChance = (double)1 / 3;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }

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

        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath($@"TR1\SecretMapping\{level.Name}-SecretMapping.json"));

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P); // Sprite/model not available
        stdItemTypes.Remove(TR1Type.Pistols_S_P); // A bit cruel as a reward?

        int secretRoom = RoomWaterUtilities.DefaultRoomCountDictionary[level.Name];
        List<Location> rewardPositions = secretMapping.Rooms.First().RewardPositions;
        List<int> rewardIndices = new(secretMapping.RewardEntities);

        // Give at least one item per secret, never less than the original reward item count,
        // and potentially some extra bonus items.
        int rewardCount = Math.Max(rewardIndices.Count, level.Script.NumSecrets);
        rewardCount = Math.Max(rewardCount, _minRewardCount);

        if (_generator.NextDouble() < _extraRewardChance)
        {
            rewardCount += level.Script.NumSecrets;
        }
        else if (_generator.NextDouble() < _doubleRewardChance)
        {
            rewardCount += 2 * level.Script.NumSecrets;
        }

        while (rewardIndices.Count < rewardCount)
        {
            TR1Entity item = ItemFactory.CreateItem(level.Name, level.Data.Entities, rewardPositions[_generator.Next(0, rewardPositions.Count)], true);
            rewardIndices.Add(level.Data.Entities.IndexOf(item));
            item.Room = (short)secretRoom;
        }

        foreach (int rewardIndex in rewardIndices)
        {
            level.Data.Entities[rewardIndex].TypeID = stdItemTypes[_generator.Next(0, stdItemTypes.Count)];
        }
    }
}
