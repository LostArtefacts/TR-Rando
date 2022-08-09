using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertEnemyFunction : BaseEMFunction
    {
        public List<int> EntityIndices { get; set; }
        public EnemyType NewEnemyType { get; set; }
        public List<short> Exclusions { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            List<TREntities> potentialTypes = TR1EntityUtilities.GetFullListOfEnemies();
            if (NewEnemyType == EnemyType.Land)
            {
                potentialTypes.RemoveAll(e => TR1EntityUtilities.IsWaterCreature(e));
            }
            else
            {
                potentialTypes.RemoveAll(e => !TR1EntityUtilities.IsWaterCreature(e));
            }

            if (Exclusions != null && Exclusions.Count > 0)
            {
                potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
            }

            TREntity enemyMatch = level.Entities.ToList().Find(e => potentialTypes.Contains((TREntities)e.TypeID));
            if (enemyMatch != null)
            {
                EMLevelData data = GetData(level);
                foreach (int index in EntityIndices)
                {
                    level.Entities[data.ConvertEntity(index)].TypeID = enemyMatch.TypeID;
                }
            }
        }

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
                potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
            }

            TR2Entity enemyMatch = level.Entities.ToList().Find(e => potentialTypes.Contains((TR2Entities)e.TypeID));
            if (enemyMatch != null)
            {
                EMLevelData data = GetData(level);
                foreach (int index in EntityIndices)
                {
                    level.Entities[data.ConvertEntity(index)].TypeID = enemyMatch.TypeID;
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TR3Entities> potentialTypes = TR3EntityUtilities.GetFullListOfEnemies();
            if (NewEnemyType == EnemyType.Land)
            {
                potentialTypes.RemoveAll(e => TR3EntityUtilities.IsWaterCreature(e));
            }
            else
            {
                potentialTypes.RemoveAll(e => !TR3EntityUtilities.IsWaterCreature(e));
            }

            if (Exclusions != null && Exclusions.Count > 0)
            {
                potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
            }

            TR2Entity enemyMatch = level.Entities.ToList().Find(e => potentialTypes.Contains((TR3Entities)e.TypeID));
            if (enemyMatch != null)
            {
                EMLevelData data = GetData(level);
                foreach (int index in EntityIndices)
                {
                    level.Entities[data.ConvertEntity(index)].TypeID = enemyMatch.TypeID;
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