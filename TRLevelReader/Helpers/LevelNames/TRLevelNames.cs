using System.Collections.Generic;

namespace TRLevelReader.Helpers
{
    public static class TRLevelNames
    {
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
        public const string ASSAULT      = "GYM.PHD";

        public const string QUALOPEC_CUT   = "CUT1.PHD";
        public const string TIHOCAN_CUT  = "CUT2.PHD";
        public const string MINES_CUT    = "CUT3.PHD";
        public const string ATLANTIS_CUT = "CUT4.PHD";

        public const string EGYPT        = "EGYPT.PHD";
        public const string CAT          = "CAT.PHD";
        public const string STRONGHOLD   = "END.PHD";
        public const string HIVE         = "END2.PHD";

        public static List<string> AsList
        {
            get
            {
                return new List<string>
                {
                    CAVES,
                    VILCABAMBA,
                    VALLEY,
                    QUALOPEC,
                    FOLLY,
                    COLOSSEUM,
                    MIDAS,
                    CISTERN,
                    TIHOCAN,
                    KHAMOON,
                    OBELISK,
                    SANCTUARY,
                    MINES,
                    ATLANTIS,
                    PYRAMID
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
                    EGYPT,
                    CAT,
                    STRONGHOLD,
                    HIVE
                };
            }
        }

        public static List<string> AsOrderedList
        {
            get
            {
                List<string> lvls = new List<string>
                {
                    CAVES,
                    VILCABAMBA,
                    VALLEY,
                    QUALOPEC,
                    QUALOPEC_CUT,
                    FOLLY,
                    COLOSSEUM,
                    MIDAS,
                    CISTERN,
                    TIHOCAN,
                    TIHOCAN_CUT,
                    KHAMOON,
                    OBELISK,
                    SANCTUARY,
                    MINES,
                    MINES_CUT,
                    ATLANTIS,
                    ATLANTIS_CUT,
                    PYRAMID
                };
                lvls.Add(ASSAULT);
                lvls.AddRange(AsListGold);
                return lvls;
            }
        }
    }
}