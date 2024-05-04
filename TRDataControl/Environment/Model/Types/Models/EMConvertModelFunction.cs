using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertModelFunction : BaseEMFunction
{
    public uint OldModelID { get; set; }
    public uint NewModelID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        ConvertModel(level.Models, level.Entities);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        ConvertModel(level.Models, level.Entities);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        ConvertModel(level.Models, level.Entities);
    }

    private void ConvertModel<T, E>(TRDictionary<T, TRModel> models, List<E> entities)
        where T : Enum
        where E : TREntity<T>
    {
        T oldID = (T)(object)OldModelID;
        T newID = (T)(object)NewModelID;
        if (!models.ChangeKey(oldID, newID))
        {
            return;
        }

        foreach (E entity in entities)
        {
            if (EqualityComparer<T>.Default.Equals(entity.TypeID, oldID))
            {
                entity.TypeID = newID;
            }
        }
    }
}
