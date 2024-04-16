using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCopySpriteSequenceFunction : BaseEMFunction
{
    public short BaseSpriteID { get; set; }
    public short TargetSpriteID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        CopySpriteSequence(level.SpriteSequences);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        CopySpriteSequence(level.SpriteSequences);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        CopySpriteSequence(level.SpriteSequences);
    }

    private void CopySpriteSequence(List<TRSpriteSequence> sequences)
    {
        TRSpriteSequence baseSequence = sequences.Find(s => s.SpriteID == BaseSpriteID);
        TRSpriteSequence targetSequence = sequences.Find(s => s.SpriteID == TargetSpriteID);

        if (baseSequence != null && targetSequence == null)
        {
            sequences.Add(new()
            {
                NegativeLength = baseSequence.NegativeLength,
                Offset = baseSequence.Offset,
                SpriteID = TargetSpriteID
            });
        }
    }
}
