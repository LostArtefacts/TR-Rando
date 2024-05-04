using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCopySpriteSequenceFunction : BaseEMFunction
{
    public short BaseSpriteID { get; set; }
    public short TargetSpriteID { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        CopySpriteSequence(level.Sprites);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        CopySpriteSequence(level.Sprites);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        CopySpriteSequence(level.Sprites);
    }

    private void CopySpriteSequence<T>(TRDictionary<T, TRSpriteSequence> sequences)
        where T : Enum
    {
        T baseID = (T)(object)(uint)BaseSpriteID;
        T targetID = (T)(object)(uint)TargetSpriteID;
        if (!sequences.ContainsKey(baseID) || sequences.ContainsKey(targetID))
        {
            return;
        }

        sequences[targetID] = sequences[baseID];
    }
}
