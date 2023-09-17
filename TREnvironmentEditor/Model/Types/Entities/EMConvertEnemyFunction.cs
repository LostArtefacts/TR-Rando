using TREnvironmentEditor.Helpers;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertEnemyFunction : BaseEMFunction
{
    public List<int> EntityIndices { get; set; }
    public EnemyType NewEnemyType { get; set; }
    public List<short> Exclusions { get; set; }
    public short PreferredType { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<TR1Type> potentialTypes = TR1EntityUtilities.GetFullListOfEnemies();
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

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TREntity enemyMatch = null;
        List<TREntity> entities = level.Entities.ToList();
        if (potentialTypes.Contains((TR1Type)PreferredType))
        {
            enemyMatch = entities.Find(e => e.TypeID == PreferredType && !EntityIndices.Contains(entities.IndexOf(e)));
        }
        enemyMatch ??= entities.Find(e => potentialTypes.Contains((TR1Type)e.TypeID));

        if (enemyMatch != null)
        {
            foreach (int index in EntityIndices)
            {
                level.Entities[index].TypeID = enemyMatch.TypeID;
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

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TR2Entity enemyMatch = null;
        List<TR2Entity> entities = level.Entities.ToList();
        if (potentialTypes.Contains((TR2Entities)PreferredType))
        {
            enemyMatch = entities.Find(e => e.TypeID == PreferredType && !EntityIndices.Contains(entities.IndexOf(e)));
        }
        enemyMatch ??= entities.Find(e => potentialTypes.Contains((TR2Entities)e.TypeID));

        if (enemyMatch != null)
        {
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

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TR2Entity enemyMatch = null;
        List<TR2Entity> entities = level.Entities.ToList();
        if (potentialTypes.Contains((TR3Entities)PreferredType))
        {
            enemyMatch = entities.Find(e => e.TypeID == PreferredType && !EntityIndices.Contains(entities.IndexOf(e)));
        }
        enemyMatch ??= entities.Find(e => potentialTypes.Contains((TR3Entities)e.TypeID));

        if (enemyMatch != null)
        {
            foreach (int index in EntityIndices)
            {
                level.Entities[data.ConvertEntity(index)].TypeID = enemyMatch.TypeID;
            }
        }
    }

    private void ConvertIndices(EMLevelData data)
    {
        for (int i = 0; i < EntityIndices.Count; i++)
        {
            EntityIndices[i] = data.ConvertEntity(EntityIndices[i]);
        }
    }
}

public enum EnemyType
{
    Water = 0,
    Land = 1
}
