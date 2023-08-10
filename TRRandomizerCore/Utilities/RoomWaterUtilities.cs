using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities;

public static class RoomWaterUtilities
{
    public static Dictionary<string, List<List<int>>> RoomRemovalWaterMap = new()
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

    public static Dictionary<string, int> DefaultRoomCountDictionary = new()
    {
        { TR1LevelNames.CAVES, 38 },
        { TR1LevelNames.VILCABAMBA, 94 },
        { TR1LevelNames.VALLEY, 91 },
        { TR1LevelNames.QUALOPEC, 54 },
        { TR1LevelNames.FOLLY, 63 },
        { TR1LevelNames.COLOSSEUM, 89 },
        { TR1LevelNames.MIDAS, 79 },
        { TR1LevelNames.CISTERN, 139 },
        { TR1LevelNames.TIHOCAN, 114 },
        { TR1LevelNames.KHAMOON, 71 },
        { TR1LevelNames.OBELISK, 84 },
        { TR1LevelNames.SANCTUARY, 64 },
        { TR1LevelNames.MINES, 108 },
        { TR1LevelNames.ATLANTIS, 101 },
        { TR1LevelNames.PYRAMID, 67 },
        { TR1LevelNames.ASSAULT, 18 },

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
        FDControl floorData = new();
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
