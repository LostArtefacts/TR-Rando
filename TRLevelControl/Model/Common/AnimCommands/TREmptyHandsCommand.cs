namespace TRLevelControl.Model;

public class TREmptyHandsCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.EmptyHands;

    public override TRAnimCommand Clone()
        => (TREmptyHandsCommand)MemberwiseClone();
}
