namespace TRLevelControl.Model;

public class TRZoneGroup : ICloneable
{
    public TRZone FlipOffZone { get; set; } = new();
    public TRZone FlipOnZone { get; set; } = new();

    public TRZoneGroup Clone()
    {
        return new()
        {
            FlipOffZone = FlipOffZone.Clone(),
            FlipOnZone = FlipOnZone.Clone(),
        };
    }

    object ICloneable.Clone()
        => Clone();
}
