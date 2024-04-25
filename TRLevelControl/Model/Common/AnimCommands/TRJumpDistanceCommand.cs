namespace TRLevelControl.Model;

public class TRJumpDistanceCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.JumpDistance;
    public short VerticalSpeed { get; set; }
    public short HorizontalSpeed { get; set; }

    public override TRAnimCommand Clone()
        => (TRJumpDistanceCommand)MemberwiseClone();
}
