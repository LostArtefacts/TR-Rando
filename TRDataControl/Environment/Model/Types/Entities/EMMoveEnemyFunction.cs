using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMoveEnemyFunction : BaseMoveTriggerableFunction
{
    public bool IfLandCreature { get; set; }
    public bool AttemptWaterCreature { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        var sectorEnemies = GetSectorEnemies<TR1Entity, TR1Type>(level.Entities);
        if (ShouldMoveEnemies<TR1Entity, TR1Type>(sectorEnemies, level.Entities,
            TR1TypeUtilities.IsEnemyType, TR1TypeUtilities.IsWaterCreature))
        {
            sectorEnemies.ForEach(e => RepositionTriggerable(e, level));
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        var sectorEnemies = GetSectorEnemies<TR2Entity, TR2Type>(level.Entities);
        if (ShouldMoveEnemies<TR2Entity, TR2Type>(sectorEnemies, level.Entities,
            TR2TypeUtilities.IsEnemyType, TR2TypeUtilities.IsWaterCreature))
        {
            sectorEnemies.ForEach(e => RepositionTriggerable(e, level));
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        var sectorEnemies = GetSectorEnemies<TR3Entity, TR3Type>(level.Entities);
        if (ShouldMoveEnemies<TR3Entity, TR3Type>(sectorEnemies, level.Entities,
            TR3TypeUtilities.IsEnemyType, TR3TypeUtilities.IsWaterCreature))
        {
            sectorEnemies.ForEach(e => RepositionTriggerable(e, level));
        }
    }

    private List<E> GetSectorEnemies<E, T>(List<E> allEntities)
        where E : TREntity<T>
        where T : Enum
    {
        var baseEnemy = allEntities[EntityIndex];
        return allEntities.FindAll(
            e => EqualityComparer<T>.Default.Equals(e.TypeID, baseEnemy.TypeID)
            && e.X == baseEnemy.X
            && e.Y == baseEnemy.Y
            && e.Z == baseEnemy.Z
            && e.Room == baseEnemy.Room);
    }

    private bool ShouldMoveEnemies<E, T>(List<E> enemies, List<E> allEntities, Func<T, bool> isEnemy, Func<T, bool> isWaterCreature)
        where E : TREntity<T>
        where T : Enum
    {
        var currentType = enemies.First().TypeID;
        if (!isEnemy(currentType)
            || IfLandCreature != !isWaterCreature(currentType))
        {
            return false;
        }

        if (AttemptWaterCreature
            && allEntities.Find(e => isWaterCreature(e.TypeID)) is E waterEnemy)
        {
            enemies.ForEach(e => e.TypeID = waterEnemy.TypeID);
            return false;
        }

        return true;
    }
}
