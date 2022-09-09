using System.Collections.Generic;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
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
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.DORIA, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.LQ, new List<List<int>>
                {
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.DECK, new List<List<int>>
                {
                    //No drain areas defined for now
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
                    //No drain areas defined for now
                }
            },
            { TR2LevelNames.CHICKEN, new List<List<int>>
                {
                    //No drain areas defined for now
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

        public static Dictionary<string, int> DefaultRoomCountDictionary = new Dictionary<string, int>
        {
            { TRLevelNames.CAVES, 38 },
            { TRLevelNames.VILCABAMBA, 94 },
            { TRLevelNames.VALLEY, 91 },
            { TRLevelNames.QUALOPEC, 54 },
            { TRLevelNames.FOLLY, 63 },
            { TRLevelNames.COLOSSEUM, 89 },
            { TRLevelNames.MIDAS, 79 },
            { TRLevelNames.CISTERN, 139 },
            { TRLevelNames.TIHOCAN, 114 },
            { TRLevelNames.KHAMOON, 71 },
            { TRLevelNames.OBELISK, 84 },
            { TRLevelNames.SANCTUARY, 64 },
            { TRLevelNames.MINES, 108 },
            { TRLevelNames.ATLANTIS, 101 },
            { TRLevelNames.PYRAMID, 67 },
            { TRLevelNames.ASSAULT, 18 },

            { TR3LevelNames.JUNGLE, 165 },
            { TR3LevelNames.RUINS, 224 },
            { TR3LevelNames.GANGES, 177 },
            { TR3LevelNames.CAVES, 25 },
            { TR3LevelNames.COASTAL, 198 },
            { TR3LevelNames.CRASH, 98 },
            { TR3LevelNames.MADUBU, 146 },
            { TR3LevelNames.PUNA, 30 },
            { TR3LevelNames.THAMES, 190 },
            { TR3LevelNames.ALDWYCH, 163 },
            { TR3LevelNames.LUDS, 210 },
            { TR3LevelNames.CITY, 20 },
            { TR3LevelNames.NEVADA, 181 },
            { TR3LevelNames.HSC, 186 },
            { TR3LevelNames.AREA51, 156 },
            { TR3LevelNames.ANTARC, 204 },
            { TR3LevelNames.RXTECH, 199 },
            { TR3LevelNames.TINNOS, 233 },
            { TR3LevelNames.WILLIE, 44 },
            { TR3LevelNames.HALLOWS, 68 },
            { TR3LevelNames.ASSAULT, 133 }
        };

        /// <summary>
        /// Take a location in a level and move it up until it is at the surface of water if it exists
        /// </summary>
        /// <param name="location">Location <see cref="Location"/></param>
        /// <param name="level">Level <see cref="TR2Level"/></param>
        /// <returns></returns>
        public static Location MoveToTheSurface(Location location, TR2Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            // Make sure the boat is just on the water surface
            while (level.Rooms[location.Room].ContainsWater)
            {
                // Get the room above this sector
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, floorData);
                if (sector.RoomAbove == byte.MaxValue)
                {
                    break;
                }
                // Put the boat at the bottom of the room above
                location.Y = level.Rooms[sector.RoomAbove].Info.YBottom;
                location.Room = sector.RoomAbove;
            }
            return location;

        }
    }
}