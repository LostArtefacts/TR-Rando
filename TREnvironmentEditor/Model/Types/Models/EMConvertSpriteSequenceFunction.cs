using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertSpriteSequenceFunction : BaseEMFunction
{
    public short OldSpriteID { get; set; }
    public short NewSpriteID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        ConvertSpriteSequence(level.Sprites, level.Entities);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        ConvertSpriteSequence(level.Sprites, level.Entities);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        ConvertSpriteSequence(level.Sprites, level.Entities);
    }

    private void ConvertSpriteSequence<T, E>(TRDictionary<T, TRSpriteSequence> sequences, List<E> entities)
        where T : Enum
        where E : TREntity<T>
    {
        T oldID = (T)(object)(uint)OldSpriteID;
        T newID = (T)(object)(uint)NewSpriteID;
        if (!sequences.ChangeKey(oldID, newID))
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
