namespace TRRandomizerCore.Textures;

public class TRTexInfo
{
    public Dictionary<TRTexCategory, SortedSet<ushort>> Categories { get; set; }
    public Dictionary<TRTexCategory, ushort> Defaults { get; set; }
}

public enum TRTexCategory
{
    Opaque,
    Fixed,
    Lever,
    Ladder,
    Transparent,
}
