namespace TRLevelControl.Helpers;

public static class TR1LevelNames
{
    public const string ASSAULT      = "GYM.PHD";
    public const string CAVES        = "LEVEL1.PHD";
    public const string VILCABAMBA   = "LEVEL2.PHD";
    public const string VALLEY       = "LEVEL3A.PHD";
    public const string QUALOPEC     = "LEVEL3B.PHD";
    public const string FOLLY        = "LEVEL4.PHD";
    public const string COLOSSEUM    = "LEVEL5.PHD";
    public const string MIDAS        = "LEVEL6.PHD";
    public const string CISTERN      = "LEVEL7A.PHD";
    public const string TIHOCAN      = "LEVEL7B.PHD";
    public const string KHAMOON      = "LEVEL8A.PHD";
    public const string OBELISK      = "LEVEL8B.PHD";
    public const string SANCTUARY    = "LEVEL8C.PHD";
    public const string MINES        = "LEVEL10A.PHD";
    public const string ATLANTIS     = "LEVEL10B.PHD";
    public const string PYRAMID      = "LEVEL10C.PHD";

    public const string QUALOPEC_CUT = "CUT1.PHD";
    public const string TIHOCAN_CUT  = "CUT2.PHD";
    public const string MINES_CUT    = "CUT3.PHD";
    public const string ATLANTIS_CUT = "CUT4.PHD";

    public const string EGYPT        = "EGYPT.PHD";
    public const string CAT          = "CAT.PHD";
    public const string STRONGHOLD   = "END.PHD";
    public const string HIVE         = "END2.PHD";

    public static List<string> AsList => Peru
        .Concat(Greece)
        .Concat(Egypt)
        .Concat(Atlantis)
        .ToList();

    public static List<string> AsListWithGold => AsList
        .Concat(AsListGold)
        .ToList();

    public static List<string> AsListWithAssault => AsList
        .Prepend(ASSAULT)
        .ToList();

    public static List<string> AsOrderedList => PeruWithCutscenes
        .Prepend(ASSAULT)
        .Concat(GreeceWithCutscenes)
        .Concat(Egypt)
        .Concat(AtlantisWithCutscenes)
        .Concat(AsListGold)
        .ToList();

    public static List<string> Peru => new()
    {
        CAVES,
        VILCABAMBA,
        VALLEY,
        QUALOPEC,
    };

    public static List<string> PeruWithCutscenes => new()
    {
        CAVES,
        VILCABAMBA,
        VALLEY,
        QUALOPEC,
        QUALOPEC_CUT,
    };

    public static List<string> Greece => new()
    {
        FOLLY,
        COLOSSEUM,
        MIDAS,
        CISTERN,
        TIHOCAN,
    };

    public static List<string> GreeceWithCutscenes => new()
    {
        FOLLY,
        COLOSSEUM,
        MIDAS,
        CISTERN,
        TIHOCAN,
        TIHOCAN_CUT,
    };

    public static List<string> Egypt => new()
    {
        KHAMOON,
        OBELISK,
        SANCTUARY,
    };

    public static List<string> Atlantis => new()
    {
        MINES,
        ATLANTIS,
        PYRAMID
    };

    public static List<string> AtlantisWithCutscenes => new()
    {
        MINES,
        MINES_CUT,
        ATLANTIS,
        ATLANTIS_CUT,
        PYRAMID
    };

    public static List<string> AsListGold => new()
    {
        EGYPT,
        CAT,
        STRONGHOLD,
        HIVE
    };
}
