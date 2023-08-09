using System.Collections.Generic;
using System.Linq;

namespace TRLevelControl.Helpers;

public class TR4LevelNames
{
    public const string ANGKOR          = "ANGKOR1.TR4";
    public const string IRIS_RACE       = "ANG_RACE.TR4";
    public const string SETH            = "SETTOMB1.TR4";
    public const string BURIAL          = "SETTOMB2.TR4";
    public const string VALLEY          = "JEEPCHAS.TR4";
    public const string KV5             = "JEEPCHS2.TR4";
    public const string KARNAK          = "KARNAK1.TR4";
    public const string HYPOSTYLE       = "HALL.TR4";
    public const string LAKE            = "LAKE.TR4";
    public const string SEMERKHET       = "SEMER.TR4";
    public const string GUARDIAN        = "SEMER2.TR4";
    public const string TRAIN           = "TRAIN.TR4";
    public const string ALEXANDRIA      = "ALEXHUB.TR4";
    public const string COASTAL         = "ALEXHUB2.TR4";
    public const string PHAROS          = "PALACES.TR4";
    public const string CLEOPATRA       = "PALACES2.TR4";
    public const string CATACOMBS       = "CSPLIT1.TR4";
    public const string POSEIDON        = "CSPLIT2.TR4";
    public const string LIBRARY         = "LIBRARY.TR4";
    public const string DEMETRIUS       = "LIBEND.TR4";
    public const string CITY            = "BIKEBIT.TR4";
    public const string TRENCHES        = "NUTRENCH.TR4";
    public const string TULUN           = "CORTYARD.TR4";
    public const string BAZAAR          = "LOWSTRT.TR4";
    public const string GATE            = "HIGHSTRT.TR4";
    public const string CITADEL         = "CITNEW.TR4";
    public const string SPHINX_COMPLEX  = "JOBY1A.TR4";
    public const string SPHINX_UNUSED   = "JOBY1B.TR4";
    public const string SPHINX_UNDER    = "JOBY2.TR4";
    public const string MENKAURE        = "JOBY3A.TR4";
    public const string MENKAURE_INSIDE = "JOBY3B.TR4";
    public const string MASTABAS        = "JOBY4A.TR4";
    public const string PYRAMID_OUT     = "JOBY4B.TR4";
    public const string KHUFU           = "JOBY4C.TR4";
    public const string PYRAMID_IN      = "JOBY5A.TR4";
    public const string HORUS1          = "JOBY5B.TR4";
    public const string HORUS2          = "JOBY5C.TR4";

    public const string TIMES           = "TIMES.TR4";

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

    public static List<string> Cambodia => new()
    {
        ANGKOR,
        IRIS_RACE
    };

    public static List<string> ValleyOfTheKings => new()
    {
        SETH,
        BURIAL,
        VALLEY,
        KV5
    };

    public static List<string> Karnak => new()
    {
        KARNAK,
        HYPOSTYLE,
        LAKE,
        SEMERKHET,
        GUARDIAN,
        TRAIN,
    };

    public static List<string> Alexandria => new()
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

    public static List<string> Cairo => new()
    {
        CITY,
        TRENCHES,
        TULUN,
        BAZAAR,
        GATE,
        CITADEL
    };

    public static List<string> Giza => new()
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
