using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMConvertSpriteSequenceFunction : BaseEMFunction
{
    public short OldSpriteID { get; set; }
    public short NewSpriteID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        ConvertSpriteSequence(level.SpriteSequences);
        UpdateSpriteEntities(level.Entities);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        ConvertSpriteSequence(level.SpriteSequences);
        UpdateSpriteEntities(level.Entities);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        ConvertSpriteSequence(level.SpriteSequences);
        UpdateSpriteEntities(level.Entities);
    }

    private void ConvertSpriteSequence(List<TRSpriteSequence> sequences)
    {
        if (sequences.Find(s => s.SpriteID == NewSpriteID) == null)
        {
            TRSpriteSequence oldSequence = sequences.Find(s => s.SpriteID == OldSpriteID);
            if (oldSequence != null)
            {
                oldSequence.SpriteID = NewSpriteID;
            }
        }
    }

    private void UpdateSpriteEntities(List<TR1Entity> entities)
    {
        foreach (TR1Entity entity in entities)
        {
            if (entity.TypeID == (TR1Type)OldSpriteID)
            {
                entity.TypeID = (TR1Type)NewSpriteID;
            }
        }
    }

    private void UpdateSpriteEntities(IEnumerable<TR2Entity> entities)
    {
        foreach (TR2Entity entity in entities)
        {
            if (entity.TypeID == (TR2Type)OldSpriteID)
            {
                entity.TypeID = (TR2Type)NewSpriteID;
            }
        }
    }

    private void UpdateSpriteEntities(IEnumerable<TR3Entity> entities)
    {
        foreach (TR3Entity entity in entities)
        {
            if (entity.TypeID == (TR3Type)OldSpriteID)
            {
                entity.TypeID = (TR3Type)NewSpriteID;
            }
        }
    }
}
