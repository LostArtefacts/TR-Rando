using System.Collections.Generic;
using TRLevelReader.Helpers;

namespace TR2RandomizerCore.Utilities
{
    public static class RoomWaterUtilities
    {
        public static Dictionary<string, List<List<int>>> RoomRemovalWaterMap = new Dictionary<string, List<List<int>>>
        {
            { LevelNames.GW, new List<List<int>>
                {   
                    //No drain areas defined for now
                }
            },
            { LevelNames.VENICE, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.BARTOLI, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.OPERA, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.RIG, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.DA, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.FATHOMS, new List<List<int>>
                {
                    new List<int>() { 33, 63, 64 }
                }
            },
            { LevelNames.DORIA, new List<List<int>>
                {
                    new List<int>() { 19, 42, 112 },
                    new List<int>() { 85, 94 },
                    new List<int>() { 2, 6, 7, 87, 88, 99, 100, 101, 102, 103}
                }
            },
            { LevelNames.LQ, new List<List<int>>
                {
                    new List<int>() { 28, 32, 72, 77 }
                }
            },
            { LevelNames.DECK, new List<List<int>>
                {
                    new List<int>() { 59, 93 }
                }
            },
            { LevelNames.TIBET, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.MONASTERY, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.COT, new List<List<int>>
                {
                    new List<int>() { 21, 22, 28, 82 }
                }
            },
            { LevelNames.CHICKEN, new List<List<int>>
                {
                    new List<int>() { 24, 29, 71 }
                }
            },
            { LevelNames.XIAN, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.FLOATER, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.LAIR, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { LevelNames.HOME, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            }
        };
    }
}