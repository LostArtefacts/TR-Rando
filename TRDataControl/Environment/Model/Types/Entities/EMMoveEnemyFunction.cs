using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMoveEnemyFunction : BaseMoveTriggerableFunction
{
    public bool IfLandCreature { get; set; }
    public bool AttemptWaterCreature { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TR1Entity enemy = level.Entities[EntityIndex];
        TR1Type enemyEntity = enemy.TypeID;
        bool isWaterEnemy = TR1TypeUtilities.IsWaterLandCreatureEquivalent(enemyEntity);

        // If the index doesn't point to an enemy or if we only want to move land creatures
        // but the enemy is a water creature (and vice-versa), bail out.
        if (!TR1TypeUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
        {
            return;
        }

        // If the level has water creatures available, and we want to switch it, do so.
        if (AttemptWaterCreature)
        {
            TR1Entity waterEnemy = level.Entities.Find(e => TR1TypeUtilities.IsWaterCreature(e.TypeID));
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
        TR2Type enemyEntity = enemy.TypeID;
        bool isWaterEnemy = TR2TypeUtilities.IsWaterCreature(enemyEntity);

        // If the index doesn't point to an enemy or if we only want to move land creatures
        // but the enemy is a water creature (and vice-versa), bail out.
        if (!TR2TypeUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
        {
            return;
        }

        // If the level has water creatures available, and we want to switch it, do so.
        if (AttemptWaterCreature)
        {
            TR2Entity waterEnemy = level.Entities.Find(e => TR2TypeUtilities.IsWaterCreature(e.TypeID));
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
        TR3Entity enemy = level.Entities[EntityIndex];
        TR3Type enemyEntity = enemy.TypeID;
        bool isWaterEnemy = TR3TypeUtilities.IsWaterCreature(enemyEntity);

        // If the index doesn't point to an enemy or if we only want to move land creatures
        // but the enemy is a water creature (and vice-versa), bail out.
        if (!TR3TypeUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
        {
            return;
        }

        // If the level has water creatures available, and we want to switch it, do so.
        if (AttemptWaterCreature)
        {
            TR3Entity waterEnemy = level.Entities.Find(e => TR3TypeUtilities.IsWaterCreature(e.TypeID));
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
