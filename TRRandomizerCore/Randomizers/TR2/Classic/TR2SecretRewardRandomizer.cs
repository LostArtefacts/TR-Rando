using TRGE.Core;
using TRLevelControl.Helpers;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretRewardRandomizer : BaseTR2Randomizer
{
    private const int _minQuantity = 4;
    private const int _maxQuantity = 12;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        Process(RandomizeRewards);
        ScriptEditor.SaveScript();
    }

    private void RandomizeRewards(TR2CombinedLevel level)
    {
        int levelQty = level.Script.Sequences.Where(s => s.Type == LevelSequenceType.Add_Secret_Reward)
            .Sum(s => ((GiveItemLevelSequence)s).Quantity);
        level.Script.Sequences.RemoveAll(s => s.Type == LevelSequenceType.Add_Secret_Reward);
        if (!level.Data.Entities.Any(e => TR2TypeUtilities.IsSecretType(e.TypeID)))
        {
            return;
        }

        var rewardTypes = TR2TypeUtilities.GetAmmoTypes();
        var gunTypes = TR2TypeUtilities.GetGunTypes();
        var count = _generator.Next(Math.Max(_minQuantity, levelQty), _maxQuantity);
        var assignment = rewardTypes.RandomSelection(_generator, count, allowDuplicates: true)
            .GroupBy(a => a)
            .SelectMany(g =>
            {
                if (!gunTypes.Contains(g.Key) || g.Count() <= 1)
                {
                    return g;
                }

                return Enumerable.Repeat(g.Key, 1)
                    .Concat(Enumerable.Repeat(TR2TypeUtilities.GetWeaponAmmo(g.Key), g.Count() - 1));
            });

        foreach (var item in assignment.GroupBy(a => a))
        {
            level.Script.AddSequenceBefore(LevelSequenceType.Loop_Game, new GiveItemLevelSequence
            {
                Type = LevelSequenceType.Add_Secret_Reward,
                ObjectId = (int)item.Key,
                Quantity = item.Count(),
            });
        }
    }
}
