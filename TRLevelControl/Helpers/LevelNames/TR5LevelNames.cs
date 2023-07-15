using System.Collections.Generic;
using System.Linq;

namespace TRLevelControl.Helpers
{
    public class TR5LevelNames
    {
        public const string ROME      = "ANDREA1.TRC";
        public const string MARKETS   = "ANDREA2.TRC";
        public const string COLOSSEUM = "ANDREA3.TRC";
        public const string BASE      = "JOBY2.TRC";
        public const string SUBMARINE = "JOBY3.TRC";
        public const string DEEPSEA   = "JOBY4.TRC";
        public const string SINKING   = "JOBY5.TRC";
        public const string GALLOWS   = "ANDY1.TRC";
        public const string LABYRINTH = "ANDY2.TRC";
        public const string MILL      = "ANDY3.TRC";
        public const string FLOOR13   = "RICH1.TRC";
        public const string ESCAPE    = "RICH2.TRC";
        public const string REDALERT  = "RICH3.TRC";

        public static List<string> AsList => Rome
            .Concat(Russia)
            .Concat(Ireland)
            .Concat(VCI)
            .ToList();

        public static List<string> Rome => new List<string>
        {
            ROME,
            MARKETS,
            COLOSSEUM
        };

        public static List<string> Russia => new List<string>
        {
            BASE,
            SUBMARINE,
            DEEPSEA,
            SINKING
        };

        public static List<string> Ireland => new List<string>
        {
            GALLOWS,
            LABYRINTH,
            MILL
        };

        public static List<string> VCI => new List<string>
        {
            FLOOR13,
            ESCAPE,
            REDALERT
        };
    }
}
