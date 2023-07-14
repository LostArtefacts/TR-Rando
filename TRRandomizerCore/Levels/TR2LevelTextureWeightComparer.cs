using System.Collections.Generic;
using TRLevelControl.Helpers;

namespace TRRandomizerCore.Levels
{
    public class TR2LevelTextureWeightComparer : IComparer<TR2CombinedLevel>
    {
        public int Compare(TR2CombinedLevel lvl1, TR2CombinedLevel lvl2)
        {
            int enemyCount1 = TR2EntityUtilities.GetEnemyTypeDictionary()[lvl1.Name].Count;
            int enemyCount2 = TR2EntityUtilities.GetEnemyTypeDictionary()[lvl2.Name].Count;

            int freeTiles1 = 16 - (int)lvl1.Data.NumImages;
            int freeTiles2 = 16 - (int)lvl2.Data.NumImages;

            if (freeTiles1 == freeTiles2)
            {
                return enemyCount2.CompareTo(enemyCount1);
            }

            return (enemyCount1 * freeTiles1).CompareTo(enemyCount2 * freeTiles2);
        }
    }
}