using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1SecretRewardAllocator
{
    private static readonly int _minRewardCount = 3;
    private static readonly double _doubleRewardChance = (double)1 / 6;
    private static readonly double _extraRewardChance = (double)1 / 3;

    public Random Generator { get; set; }
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public void RandomizeRewards(string levelName, TR1Level level)
    {
        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get($@"Resources\TR1\SecretMapping\{levelName}-SecretMapping.json");
        if (secretMapping == null)
        {
            return;
        }

        int numSecrets = level.FloorData.GetActionItems(FDTrigAction.SecretFound)
            .Select(a => a.Parameter)
            .Distinct().Count();

        List<TR1Type> stdItemTypes = TR1TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR1Type.PistolAmmo_S_P);
        stdItemTypes.Remove(TR1Type.Pistols_S_P);

        List<int> rewardIndices = new(secretMapping.RewardEntities);

        // Pile extra pickups on top of existing ones, either in their default spots
        // or in the generated reward rooms (for classic only).
        List<Location> rewardPositions = new(rewardIndices.Select(i => level.Entities[i].GetLocation()));

        // Give at least one item per secret, never less than the original reward item count,
        // and potentially some extra bonus items.
        int rewardCount = Math.Max(rewardIndices.Count, numSecrets);
        rewardCount = Math.Max(rewardCount, _minRewardCount);

        if (Generator.NextDouble() < _extraRewardChance)
        {
            rewardCount += numSecrets;
        }
        else if (Generator.NextDouble() < _doubleRewardChance)
        {
            rewardCount += 2 * numSecrets;
        }

        while (rewardIndices.Count < rewardCount)
        {
            Location location = rewardPositions[Generator.Next(0, rewardPositions.Count)];
            TR1Entity item = ItemFactory.CreateItem(levelName, level.Entities, location, true);
            rewardIndices.Add(level.Entities.IndexOf(item));
            item.Room = location.Room;
        }

        foreach (int rewardIndex in rewardIndices)
        {
            level.Entities[rewardIndex].TypeID = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
        }
    }
}
