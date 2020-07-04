using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Helpers
{
    public static class LevelNames
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
    }
}
