namespace TRLevelControl.Model;

public class TRZone : ICloneable
{
    public Dictionary<TRZoneType, ushort> Ground { get; set; } = new();
    public ushort Fly { get; set; }

    public TRZone Clone()
    {
        return new()
        {
            Ground = Ground.ToDictionary(e => e.Key, e => e.Value),
            Fly = Fly
        };
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
