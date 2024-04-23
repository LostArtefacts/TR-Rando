namespace TRLevelControl.Model;

public class TRAnimCommand : ICloneable
{
    public TRAnimCommandType Type { get; set; }
    public List<short> Params { get; set; } = new();

    public TRAnimCommand Clone()
    {
        return new()
        {
            Type = Type,
            Params = new(Params),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
