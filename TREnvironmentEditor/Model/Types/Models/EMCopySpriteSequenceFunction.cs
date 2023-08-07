using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCopySpriteSequenceFunction : BaseEMFunction
{
    public short BaseSpriteID { get; set; }
    public short TargetSpriteID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        List<TRSpriteSequence> sequences = level.SpriteSequences.ToList();
        CopySpriteSequence(sequences);
        level.SpriteSequences = sequences.ToArray();
        level.NumSpriteSequences = (uint)sequences.Count;
    }

    public override void ApplyToLevel(TR2Level level)
    {
        List<TRSpriteSequence> sequences = level.SpriteSequences.ToList();
        CopySpriteSequence(sequences);
        level.SpriteSequences = sequences.ToArray();
        level.NumSpriteSequences = (uint)sequences.Count;
    }

    public override void ApplyToLevel(TR3Level level)
    {
        List<TRSpriteSequence> sequences = level.SpriteSequences.ToList();
        CopySpriteSequence(sequences);
        level.SpriteSequences = sequences.ToArray();
        level.NumSpriteSequences = (uint)sequences.Count;
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
