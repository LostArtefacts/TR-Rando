using System.Linq;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveEnemyFunction : BaseMoveTriggerableFunction
    {
        public bool IfLandCreature { get; set; }
        public bool AttemptWaterCreature { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            TREntity enemy = level.Entities[EntityIndex];
            TREntities enemyEntity = (TREntities)enemy.TypeID;
            bool isWaterEnemy = TR1EntityUtilities.IsWaterCreature(enemyEntity);

            // If the index doesn't point to an enemy or if we only want to move land creatures
            // but the enemy is a water creature (and vice-versa), bail out.
            if (!TR1EntityUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
            {
                return;
            }

            // If the level has water creatures available, and we want to switch it, do so.
            if (AttemptWaterCreature)
            {
                TREntity waterEnemy = level.Entities.ToList().Find(e => TR1EntityUtilities.IsWaterCreature((TREntities)e.TypeID));
                if (waterEnemy != null)
                {
                    enemy.TypeID = waterEnemy.TypeID;
                    return;
                }
            }

            // Otherwise, reposition the enemy and its triggers.
            RepositionTriggerable(enemy, level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            TR2Entity enemy = level.Entities[EntityIndex];
            TR2Entities enemyEntity = (TR2Entities)enemy.TypeID;
            bool isWaterEnemy = TR2EntityUtilities.IsWaterCreature(enemyEntity);

            // If the index doesn't point to an enemy or if we only want to move land creatures
            // but the enemy is a water creature (and vice-versa), bail out.
            if (!TR2EntityUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
            {
                return;
            }

            // If the level has water creatures available, and we want to switch it, do so.
            if (AttemptWaterCreature)
            {
                TR2Entity waterEnemy = level.Entities.ToList().Find(e => TR2EntityUtilities.IsWaterCreature((TR2Entities)e.TypeID));
                if (waterEnemy != null)
                {
                    enemy.TypeID = waterEnemy.TypeID;
                    return;
                }
            }

            // Otherwise, reposition the enemy and its triggers.
            RepositionTriggerable(enemy, level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            TR2Entity enemy = level.Entities[EntityIndex];
            TR3Entities enemyEntity = (TR3Entities)enemy.TypeID;
            bool isWaterEnemy = TR3EntityUtilities.IsWaterCreature(enemyEntity);

            // If the index doesn't point to an enemy or if we only want to move land creatures
            // but the enemy is a water creature (and vice-versa), bail out.
            if (!TR3EntityUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
            {
                return;
            }

            // If the level has water creatures available, and we want to switch it, do so.
            if (AttemptWaterCreature)
            {
                TR2Entity waterEnemy = level.Entities.ToList().Find(e => TR3EntityUtilities.IsWaterCreature((TR3Entities)e.TypeID));
                if (waterEnemy != null)
                {
                    enemy.TypeID = waterEnemy.TypeID;
                    return;
                }
            }

            // Otherwise, reposition the enemy and its triggers.
            RepositionTriggerable(enemy, level);
        }
    }
}