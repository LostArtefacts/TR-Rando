using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRewardAllocator
{
    public Random Generator { get; set; }

    public void RandomizeRewards(TR3Level level, List<int> rewardEntities)
    {
        List<TR3Type> stdItemTypes = TR3TypeUtilities.GetStandardPickupTypes();
        stdItemTypes.Remove(TR3Type.PistolAmmo_P);
        stdItemTypes.Remove(TR3Type.Pistols_P);

        foreach (int rewardIndex in rewardEntities)
        {
            if (level.Entities[rewardIndex].TypeID == TR3Type.SaveCrystal_P)
            {
                continue;
            }

            level.Entities[rewardIndex].TypeID = stdItemTypes[Generator.Next(0, stdItemTypes.Count)];
        }
    }
}
