using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader;
using TRLevelReader.Model;

namespace TR1_NormalJumping
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level1.phd");

            lvl.AnimDispatches[4].Low = 11;
            lvl.AnimDispatches[4].High = 22;

            lvl.AnimDispatches[5].Low = 0;
            lvl.AnimDispatches[5].High = 11;

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level1.phd");
        }
    }
}
