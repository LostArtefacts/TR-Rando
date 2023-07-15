using System.Collections.Generic;
using System.Linq;

namespace TRLevelControl.Helpers
{
    public class TR4LevelNames
    {
        public static readonly string ANGKOR          = "ANGKOR1.TR4";
        public static readonly string IRIS_RACE       = "ANG_RACE.TR4";
        public static readonly string SETH            = "SETTOMB1.TR4";
        public static readonly string BURIAL          = "SETTOMB2.TR4";
        public static readonly string VALLEY          = "JEEPCHAS.TR4";
        public static readonly string KV5             = "JEEPCHS2.TR4";
        public static readonly string KARNAK          = "KARNAK1.TR4";
        public static readonly string HYPOSTYLE       = "HALL.TR4";
        public static readonly string LAKE            = "LAKE.TR4";
        public static readonly string SEMERKHET       = "SEMER.TR4";
        public static readonly string GUARDIAN        = "SEMER2.TR4";
        public static readonly string TRAIN           = "TRAIN.TR4";
        public static readonly string ALEXANDRIA      = "ALEXHUB.TR4";
        public static readonly string COASTAL         = "ALEXHUB2.TR4";
        public static readonly string PHAROS          = "PALACES.TR4";
        public static readonly string CLEOPATRA       = "PALACES2.TR4";
        public static readonly string CATACOMBS       = "CSPLIT1.TR4";
        public static readonly string POSEIDON        = "CSPLIT2.TR4";
        public static readonly string LIBRARY         = "LIBRARY.TR4";
        public static readonly string DEMETRIUS       = "LIBEND.TR4";
        public static readonly string CITY            = "BIKEBIT.TR4";
        public static readonly string TRENCHES        = "NUTRENCH.TR4";
        public static readonly string TULUN           = "CORTYARD.TR4";
        public static readonly string BAZAAR          = "LOWSTRT.TR4";
        public static readonly string GATE            = "HIGHSTRT.TR4";
        public static readonly string CITADEL         = "CITNEW.TR4";
        public static readonly string SPHINX_COMPLEX  = "JOBY1A.TR4";
        public static readonly string SPHINX_UNUSED   = "JOBY1B.TR4";
        public static readonly string SPHINX_UNDER    = "JOBY2.TR4";
        public static readonly string MENKAURE        = "JOBY3A.TR4";
        public static readonly string MENKAURE_INSIDE = "JOBY3B.TR4";
        public static readonly string MASTABAS        = "JOBY4A.TR4";
        public static readonly string PYRAMID_OUT     = "JOBY4B.TR4";
        public static readonly string KHUFU           = "JOBY4C.TR4";
        public static readonly string PYRAMID_IN      = "JOBY5A.TR4";
        public static readonly string HORUS1          = "JOBY5B.TR4";
        public static readonly string HORUS2          = "JOBY5C.TR4";

        public static readonly string TIMES           = "TIMES.TR4";

        public static List<string> AsList => Cambodia
            .Concat(ValleyOfTheKings)
            .Concat(Karnak)
            .Concat(Alexandria)
            .Concat(Cairo)
            .Concat(Giza)
            .ToList();

        public static List<string> AsOrderedList => AsList
            .Append(TIMES)
            .ToList();

        public static List<string> Cambodia => new List<string>
        {
            ANGKOR,
            IRIS_RACE
        };

        public static List<string> ValleyOfTheKings => new List<string>
        {
            SETH,
            BURIAL,
            VALLEY,
            KV5
        };

        public static List<string> Karnak => new List<string>
        {
            KARNAK,
            HYPOSTYLE,
            LAKE,
            SEMERKHET,
            GUARDIAN
        };

        public static List<string> Alexandria => new List<string>
        {
            ALEXANDRIA,
            COASTAL,
            PHAROS,
            CLEOPATRA,
            CATACOMBS,
            POSEIDON,
            LIBRARY,
            DEMETRIUS,
        };

        public static List<string> Cairo => new List<string>
        {
            CITY,
            TRENCHES,
            TULUN,
            BAZAAR,
            GATE,
            CITADEL
        };

        public static List<string> Giza => new List<string>
        {
            SPHINX_COMPLEX,
            SPHINX_UNDER,
            MENKAURE,
            MENKAURE_INSIDE,
            MASTABAS,
            PYRAMID_OUT,
            KHUFU,
            PYRAMID_IN,
            HORUS1,
            HORUS2
        };
    }
}
