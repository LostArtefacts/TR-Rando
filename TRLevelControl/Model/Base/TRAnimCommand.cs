namespace TRLevelControl.Model;

public abstract class TRAnimCommand : ICloneable
{
    public abstract TRAnimCommandType Type { get; }

    public abstract TRAnimCommand Clone();

    object ICloneable.Clone()
        => Clone();
}
