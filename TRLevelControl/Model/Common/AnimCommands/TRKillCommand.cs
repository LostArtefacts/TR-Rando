namespace TRLevelControl.Model;

public class TRKillCommand : TRAnimCommand
{
    public override TRAnimCommandType Type => TRAnimCommandType.Kill;

    public override TRAnimCommand Clone()
        => (TRKillCommand)MemberwiseClone();
}
