namespace TRLevelControl.Model;

public class TRStateChange : ICloneable
{
    public ushort StateID { get; set; }
    public List<TRAnimDispatch> Dispatches { get; set; } = new();

    public TRStateChange Clone()
    {
        return new()
        {
            StateID = StateID,
            Dispatches = new(Dispatches.Select(c => c.Clone())),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
