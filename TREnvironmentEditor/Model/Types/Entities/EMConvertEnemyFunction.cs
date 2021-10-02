using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertEnemyFunction : BaseEMFunction
    {
        public List<int> EntityIndices { get; set; }
        public EnemyType NewEnemyType { get; set; }
        public List<TR2Entities> Exclusions { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            // Find the first instance of an existing enemy of the same type
            // we want to convert to. If none found, no action is taken.
            List<TR2Entities> potentialTypes = TR2EntityUtilities.GetFullListOfEnemies();
            if (NewEnemyType == EnemyType.Land)
            {
                potentialTypes.RemoveAll(e => TR2EntityUtilities.IsWaterCreature(e));
            }
            else
            {
                potentialTypes.RemoveAll(e => !TR2EntityUtilities.IsWaterCreature(e));
            }

            if (Exclusions != null && Exclusions.Count > 0)
            {
                potentialTypes.RemoveAll(e => Exclusions.Contains(e));
            }

            TR2Entity enemyMatch = level.Entities.ToList().Find(e => potentialTypes.Contains((TR2Entities)e.TypeID));
            if (enemyMatch != null)
            {
                foreach (int index in EntityIndices)
                {
                    level.Entities[index].TypeID = enemyMatch.TypeID;
                }
            }
        }
    }

    public enum EnemyType
    {
        Water = 0,
        Land = 1
    }
}