namespace TRRandomizerCore.Textures;

public class TRTexInfo<T>
    where T : Enum
{
    public Dictionary<TRTexCategory, SortedSet<ushort>> Categories { get; set; }
    public Dictionary<TRTexCategory, ushort> Defaults { get; set; }
    public List<Dictionary<string, Dictionary<T, TRItemFlags>>> ItemFlags { get; set; }
}

public enum TRTexCategory
{
    Opaque,
    Fixed,
    Lever,
    Ladder,
    Transparent,
}

[Flags]
public enum TRItemFlags
{
    LeftDoor         = 1 << 0,
    RightDoor        = 1 << 1,
    LiftingDoor      = 1 << 2,
    Trapdoor         = 1 << 3,
    FourClick        = 1 << 4,
    FiveClick        = 1 << 5,
    SixClick         = 1 << 6,
    EightClick       = 1 << 7,
    Drawbridge       = 1 << 8,
    
    FallingBlock     = 1 << 9,
    PushBlock        = 1 << 10,
    SlammingDoor     = 1 << 11,
    WallSwitch       = 1 << 12,
    UnderwaterSwitch = 1 << 13,
    BreakableWindow  = 1 << 14,
    PushButton       = 1 << 15,
    Springboard      = 1 << 16,
    Boulder          = 1 << 17,
    Darts            = 1 << 18,
    Spikes           = 1 << 19,
    Spindle          = 1 << 20,

    PairA            = 1 << 24,
    PairB            = 1 << 25,
    PairC            = 1 << 26,
}
