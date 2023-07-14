using System.Collections.Generic;

namespace TRLevelReader.Helpers
{
    public static class TR2LevelNames
    {
        public const string GW          = "WALL.TR2";
        public const string VENICE      = "BOAT.TR2";
        public const string BARTOLI     = "VENICE.TR2";
        public const string OPERA       = "OPERA.TR2";
        public const string RIG         = "RIG.TR2";
        public const string DA          = "PLATFORM.TR2";
        public const string FATHOMS     = "UNWATER.TR2";
        public const string DORIA       = "KEEL.TR2";
        public const string LQ          = "LIVING.TR2";
        public const string DECK        = "DECK.TR2";
        public const string TIBET       = "SKIDOO.TR2";
        public const string MONASTERY   = "MONASTRY.TR2";
        public const string COT         = "CATACOMB.TR2";
        public const string CHICKEN     = "ICECAVE.TR2";
        public const string XIAN        = "EMPRTOMB.TR2";
        public const string FLOATER     = "FLOATING.TR2";
        public const string LAIR        = "XIAN.TR2";
        public const string HOME        = "HOUSE.TR2";
        public const string ASSAULT     = "ASSAULT.TR2";

        public const string GW_CUT      = "CUT1.TR2";
        public const string OPERA_CUT   = "CUT2.TR2";
        public const string DA_CUT      = "CUT3.TR2";
        public const string XIAN_CUT    = "CUT4.TR2";

        public const string COLDWAR     = "level1.TR2";
        public const string FOOLGOLD    = "level2.TR2";
        public const string FURNACE     = "level3.TR2";
        public const string KINGDOM     = "level4.tr2";
        public const string VEGAS       = "level5.tr2";

        public static List<string> AsList
        {
            get
            {
                return new List<string>
                {
                    GW,
                    VENICE,
                    BARTOLI,
                    OPERA,
                    RIG,
                    DA,
                    FATHOMS,
                    DORIA,
                    LQ,
                    DECK,
                    TIBET,
                    MONASTERY,
                    COT,
                    CHICKEN,
                    XIAN,
                    FLOATER,
                    LAIR,
                    HOME
                };
            }
        }

        public static List<string> AsListWithAssault
        {
            get
            {
                List<string> lvls = AsList;
                lvls.Add(ASSAULT);
                return lvls;
            }
        }

        public static List<string> AsListGold
        {
            get
            {
                return new List<string>
                {
                    COLDWAR,
                    FOOLGOLD,
                    FURNACE,
                    KINGDOM,
                    VEGAS
                };
            }
        }

        public static List<string> AsOrderedList
        {
            get
            {
                List<string> lvls = new List<string>
                {
                    GW,
                    GW_CUT,
                    VENICE,
                    BARTOLI,
                    OPERA,
                    OPERA_CUT,
                    RIG,
                    DA,
                    DA_CUT,
                    FATHOMS,
                    DORIA,
                    LQ,
                    DECK,
                    TIBET,
                    MONASTERY,
                    COT,
                    CHICKEN,
                    XIAN,
                    XIAN_CUT,
                    FLOATER,
                    LAIR,
                    HOME
                };
                lvls.Add(ASSAULT);
                lvls.AddRange(AsListGold);
                return lvls;
            }
        }
    }
}