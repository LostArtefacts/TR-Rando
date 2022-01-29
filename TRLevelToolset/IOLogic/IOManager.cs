using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader;
using TRLevelReader.Model;

namespace TRLevelToolset.IOLogic
{
    internal static class IOManager
    {
        internal static TRLevel currTR1Level { get; set; }
        internal static TR2Level currTR2Level { get; set; }
        internal static TR3Level currTR3Level { get; set; }
        internal static TR4Level currTR4Level { get; set; }
        internal static TR5Level currTR5Level { get; set; }

        internal static void Load(string fname, TRGame game)
        {
            switch (game)
            {
                case TRGame.TR1:
                    TR1LevelReader TR1reader = new TR1LevelReader();
                    currTR1Level = TR1reader.ReadLevel(fname);
                    break;
                case TRGame.TR2:
                    TR2LevelReader TR2reader = new TR2LevelReader();
                    currTR2Level = TR2reader.ReadLevel(fname);
                    break;
                case TRGame.TR3:
                    TR3LevelReader TR3reader = new TR3LevelReader();
                    currTR3Level = TR3reader.ReadLevel(fname);
                    break;
                case TRGame.TR4:
                    TR4LevelReader TR4reader = new TR4LevelReader();
                    currTR4Level = TR4reader.ReadLevel(fname);
                    break;
                case TRGame.TR5:
                    TR5LevelReader TR5reader = new TR5LevelReader();
                    currTR5Level = TR5reader.ReadLevel(fname);
                    break;
                default:
                    break;
            }
        }
    }

    internal enum TRGame
    {
        TR1,
        TR2,
        TR3,
        TR4,
        TR5
    }
}
