using TRLevelControl.Helpers;

namespace TRRandomizerCore.Levels;

public class TR2LevelTextureWeightComparer : IComparer<TR2CombinedLevel>
{
    public int Compare(TR2CombinedLevel lvl1, TR2CombinedLevel lvl2)
    {
        int enemyCount1 = TR2TypeUtilities.GetEnemyTypeDictionary()[lvl1.Name].Count;
        int enemyCount2 = TR2TypeUtilities.GetEnemyTypeDictionary()[lvl2.Name].Count;

        int freeTiles1 = 16 - lvl1.Data.Images8.Count;
        int freeTiles2 = 16 - lvl2.Data.Images8.Count;

        if (freeTiles1 == freeTiles2)
        {
            return enemyCount2.CompareTo(enemyCount1);
        }

        return (enemyCount1 * freeTiles1).CompareTo(enemyCount2 * freeTiles2);
    }
}
