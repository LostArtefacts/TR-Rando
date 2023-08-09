namespace TRLevelControl.Helpers;

public static class TR3LevelNames
{
    public const string ASSAULT     = "HOUSE.TR2";
    public const string JUNGLE      = "JUNGLE.TR2";
    public const string RUINS       = "TEMPLE.TR2";
    public const string GANGES      = "QUADCHAS.TR2";
    public const string CAVES       = "TONYBOSS.TR2";
    public const string COASTAL     = "SHORE.TR2";
    public const string CRASH       = "CRASH.TR2";
    public const string MADUBU      = "RAPIDS.TR2";
    public const string PUNA        = "TRIBOSS.TR2";
    public const string THAMES      = "ROOFS.TR2";
    public const string ALDWYCH     = "SEWER.TR2";
    public const string LUDS        = "TOWER.TR2";
    public const string CITY        = "OFFICE.TR2";
    public const string NEVADA      = "NEVADA.TR2";
    public const string HSC         = "COMPOUND.TR2";
    public const string AREA51      = "AREA51.TR2";
    public const string ANTARC      = "ANTARC.TR2";
    public const string RXTECH      = "MINES.TR2";
    public const string TINNOS      = "CITY.TR2";
    public const string WILLIE      = "CHAMBER.TR2";
    public const string HALLOWS     = "STPAUL.TR2";

    public const string JUNGLE_CUT  = "CUT6.TR2";
    public const string RUINS_CUT   = "CUT9.TR2";
    public const string COASTAL_CUT = "CUT1.TR2";
    public const string CRASH_CUT   = "CUT4.TR2";
    public const string THAMES_CUT  = "CUT2.TR2";
    public const string ALDWYCH_CUT = "CUT5.TR2";
    public const string LUDS_CUT    = "CUT11.TR2";
    public const string NEVADA_CUT  = "CUT7.TR2";
    public const string HSC_CUT     = "CUT8.TR2";
    public const string ANTARC_CUT  = "CUT3.TR2";
    public const string TINNOS_CUT  = "CUT12.TR2";

    public const string FLING       = "SCOTLAND.TR2";
    public const string LAIR        = "WILLSDEN.TR2";
    public const string CLIFF       = "CHUNNEL.TR2";
    public const string FISHES      = "UNDERSEA.TR2";
    public const string MADHOUSE    = "ZOO.TR2";
    public const string REUNION     = "SLINC.TR2";

    public static List<string> AsList => India
        .Concat(SouthPacific)
        .Concat(London)
        .Concat(Nevada)
        .Concat(Antarctica)
        .Append(HALLOWS)
        .ToList();

    public static List<string> AsListWithAssault => AsList
        .Prepend(ASSAULT)
        .ToList();

    public static List<string> AsOrderedList => IndiaWithCutscenes
        .Prepend(ASSAULT)
        .Concat(SouthPacificWithCutscenes)
        .Concat(LondonWithCutscenes)
        .Concat(NevadaWithCutscenes)
        .Concat(AntarcticaWithCutscenes)
        .Append(HALLOWS)
        .Concat(AsListGold)
        .ToList();

    public static List<string> India => new()
    {
        JUNGLE,
        RUINS,
        GANGES,
        CAVES
    };

    public static List<string> IndiaWithCutscenes => new()
    {
        JUNGLE,
        JUNGLE_CUT,
        RUINS,
        RUINS_CUT,
        GANGES,
        CAVES
    };

    public static List<string> SouthPacific => new()
    {
        COASTAL,
        CRASH,
        MADUBU,
        PUNA
    };

    public static List<string> SouthPacificWithCutscenes => new()
    {
        COASTAL,
        COASTAL_CUT,
        CRASH,
        CRASH_CUT,
        MADUBU,
        PUNA
    };

    public static List<string> London => new()
    {
        THAMES,
        ALDWYCH,
        LUDS,
        CITY,
        HALLOWS
    };

    public static List<string> LondonWithCutscenes => new()
    {
        THAMES,
        THAMES_CUT,
        ALDWYCH,
        ALDWYCH_CUT,
        LUDS,
        LUDS_CUT,
        CITY,
        HALLOWS
    };

    public static List<string> Nevada => new()
    {
        NEVADA,
        HSC,
        AREA51
    };

    public static List<string> NevadaWithCutscenes => new()
    {
        NEVADA,
        NEVADA_CUT,
        HSC,
        HSC_CUT,
        AREA51
    };

    public static List<string> Antarctica => new()
    {
        ANTARC,
        RXTECH,
        TINNOS,
        WILLIE
    };

    public static List<string> AntarcticaWithCutscenes => new()
    {
        ANTARC,
        ANTARC_CUT,
        RXTECH,
        TINNOS,
        TINNOS_CUT,
        WILLIE
    };

    public static List<string> AsListGold => new()
    {
        FLING,
        LAIR,
        CLIFF,
        FISHES,
        MADHOUSE,
        REUNION
    };
}
