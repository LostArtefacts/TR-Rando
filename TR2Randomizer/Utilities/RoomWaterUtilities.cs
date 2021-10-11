using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TRLevelReader.Helpers;

namespace TR2Randomizer.Utilities
{
    public static class RoomWaterUtilities
    {
        public static Dictionary<string, List<List<int>>> RoomRemovalWaterMap = new Dictionary<string, List<List<int>>>
        {
            { TR2LevelNames.GW, new List<List<int>>
                {   
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.VENICE, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.BARTOLI, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.OPERA, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.RIG, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.DA, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.FATHOMS, new List<List<int>>
                {
                    new List<int>() { 33, 63, 64 }
                }
            },
            { TR2LevelNames.DORIA, new List<List<int>>
                {
                    new List<int>() { 19, 42, 112 },
                    new List<int>() { 85, 94 },
                    new List<int>() { 2, 6, 7, 87, 88, 99, 100, 101, 102, 103}
                }
            },
            { TR2LevelNames.LQ, new List<List<int>>
                {
                    new List<int>() { 28, 32, 72, 77 }
                }
            },
            { TR2LevelNames.DECK, new List<List<int>>
                {
                    new List<int>() { 59, 93 }
                }
            },
            { TR2LevelNames.TIBET, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.MONASTERY, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.COT, new List<List<int>>
                {
                    new List<int>() { 21, 22, 28, 82 }
                }
            },
            { TR2LevelNames.CHICKEN, new List<List<int>>
                {
                    new List<int>() { 24, 29, 71 }
                }
            },
            { TR2LevelNames.XIAN, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.FLOATER, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.LAIR, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.HOME, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            }
        };
    }
}
