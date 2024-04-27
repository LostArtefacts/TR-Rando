namespace TRLevelControl.Model;

public class TRNullCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.Null;
    public short Value { get; set; }

    public override TRAnimCommand Clone()
        => (TRNullCommand)MemberwiseClone();
}
