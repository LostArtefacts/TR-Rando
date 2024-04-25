namespace TRLevelControl.Model;

public class TRSetPositionCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.SetPosition;
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public override TRAnimCommand Clone()
        => (TRSetPositionCommand)MemberwiseClone();
}
