namespace TRLevelControl.Helpers;

public static class TR2LevelNames
{
    public const string ASSAULT   = "ASSAULT.TR2";
    public const string GW        = "WALL.TR2";
    public const string VENICE    = "BOAT.TR2";
    public const string BARTOLI   = "VENICE.TR2";
    public const string OPERA     = "OPERA.TR2";
    public const string RIG       = "RIG.TR2";
    public const string DA        = "PLATFORM.TR2";
    public const string FATHOMS   = "UNWATER.TR2";
    public const string DORIA     = "KEEL.TR2";
    public const string LQ        = "LIVING.TR2";
    public const string DECK      = "DECK.TR2";
    public const string TIBET     = "SKIDOO.TR2";
    public const string MONASTERY = "MONASTRY.TR2";
    public const string COT       = "CATACOMB.TR2";
    public const string CHICKEN   = "ICECAVE.TR2";
    public const string XIAN      = "EMPRTOMB.TR2";
    public const string FLOATER   = "FLOATING.TR2";
    public const string LAIR      = "XIAN.TR2";
    public const string HOME      = "HOUSE.TR2";

    public const string GW_CUT    = "CUT1.TR2";
    public const string OPERA_CUT = "CUT2.TR2";
    public const string DA_CUT    = "CUT3.TR2";
    public const string XIAN_CUT  = "CUT4.TR2";

    public const string COLDWAR   = "LEVEL1.TR2";
    public const string FOOLGOLD  = "LEVEL2.TR2";
    public const string FURNACE   = "LEVEL3.TR2";
    public const string KINGDOM   = "LEVEL4.TR2";
    public const string VEGAS     = "LEVEL5.TR2";

    public static List<string> AsList => GreatWall
        .Concat(Italy)
        .Concat(Offshore)
        .Concat(Tibet)
        .Concat(China)
        .Append(HOME)
        .ToList();

    public static List<string> AsListWithAssault => AsList
        .Prepend(ASSAULT)
        .ToList();

    public static List<string> AsOrderedList => GreatWallWithCutscenes
        .Prepend(ASSAULT)
        .Concat(ItalyWithCutscenes)
        .Concat(OffshoreWithCutscenes)
        .Concat(Tibet)
        .Concat(ChinaWithCutscenes)
        .Append(HOME)
        .Concat(AsListGold)
        .ToList();

    public static List<string> GreatWall => new()
    {
        GW,
    };

    public static List<string> GreatWallWithCutscenes => new()
    {
        GW,
        GW_CUT
    };

    public static List<string> Italy => new()
    {
        VENICE,
        BARTOLI,
        OPERA,
    };

    public static List<string> ItalyWithCutscenes => new()
    {
        VENICE,
        BARTOLI,
        OPERA,
        OPERA_CUT
    };

    public static List<string> Offshore => new()
    {
        RIG,
        DA,
        FATHOMS,
        DORIA,
        LQ,
        DECK,
    };

    public static List<string> OffshoreWithCutscenes => new()
    {
        RIG,
        DA,
        DA_CUT,
        FATHOMS,
        DORIA,
        LQ,
        DECK,
    };

    public static List<string> Tibet => new()
    {
        TIBET,
        MONASTERY,
        COT,
        CHICKEN,
    };

    public static List<string> China => new()
    {
        XIAN,
        FLOATER,
        LAIR,
    };

    public static List<string> ChinaWithCutscenes => new()
    {
        XIAN,
        XIAN_CUT,
        FLOATER,
        LAIR,
    };

    public static List<string> AsListGold => new()
    {
        COLDWAR,
        FOOLGOLD,
        FURNACE,
        KINGDOM,
        VEGAS
    };
}
