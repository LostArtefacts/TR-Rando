using TREnvironmentEditor.Helpers;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertEnemyFunction : BaseEMFunction
{
    public List<int> EntityIndices { get; set; }
    public EnemyType NewEnemyType { get; set; }
    public List<short> Exclusions { get; set; }
    public short PreferredType { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<TR1Type> potentialTypes = TR1TypeUtilities.GetFullListOfEnemies();
        if (NewEnemyType == EnemyType.Land)
        {
            potentialTypes.RemoveAll(e => TR1TypeUtilities.IsWaterCreature(e));
        }
        else
        {
            potentialTypes.RemoveAll(e => !TR1TypeUtilities.IsWaterCreature(e));
        }

        if (Exclusions != null && Exclusions.Count > 0)
        {
            potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
        }

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TR1Entity enemyMatch = null;
        if (potentialTypes.Contains((TR1Type)PreferredType))
        {
            enemyMatch = level.Entities.Find(e => (short)e.TypeID == PreferredType && !EntityIndices.Contains(level.Entities.IndexOf(e)));
        }
        enemyMatch ??= level.Entities.Find(e => potentialTypes.Contains((TR1Type)e.TypeID));

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
        List<TR2Type> potentialTypes = TR2TypeUtilities.GetFullListOfEnemies();
        if (NewEnemyType == EnemyType.Land)
        {
            potentialTypes.RemoveAll(e => TR2TypeUtilities.IsWaterCreature(e));
        }
        else
        {
            potentialTypes.RemoveAll(e => !TR2TypeUtilities.IsWaterCreature(e));
        }

        if (Exclusions != null && Exclusions.Count > 0)
        {
            potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
        }

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TR2Entity enemyMatch = null;
        if (potentialTypes.Contains((TR2Type)PreferredType))
        {
            enemyMatch = level.Entities.Find(e => (short)e.TypeID == PreferredType && !EntityIndices.Contains(level.Entities.IndexOf(e)));
        }
        enemyMatch ??= level.Entities.Find(e => potentialTypes.Contains((TR2Type)e.TypeID));

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
        List<TR3Type> potentialTypes = TR3TypeUtilities.GetFullListOfEnemies();
        if (NewEnemyType == EnemyType.Land)
        {
            potentialTypes.RemoveAll(e => TR3TypeUtilities.IsWaterCreature(e));
        }
        else
        {
            potentialTypes.RemoveAll(e => !TR3TypeUtilities.IsWaterCreature(e));
        }

        if (Exclusions != null && Exclusions.Count > 0)
        {
            potentialTypes.RemoveAll(e => Exclusions.Contains((short)e));
        }

        EMLevelData data = GetData(level);
        ConvertIndices(data);

        TR3Entity enemyMatch = null;
        if (potentialTypes.Contains((TR3Type)PreferredType))
        {
            enemyMatch = level.Entities.Find(e => (short)e.TypeID == PreferredType && !EntityIndices.Contains(level.Entities.IndexOf(e)));
        }
        enemyMatch ??= level.Entities.Find(e => potentialTypes.Contains((TR3Type)e.TypeID));

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
