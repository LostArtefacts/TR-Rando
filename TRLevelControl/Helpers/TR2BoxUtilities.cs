using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public class TR2BoxUtilities
{
    public static int GetSectorCount(TR2Level level, int boxIndex)
    {
        int count = 0;
        foreach (TR2Room room in level.Rooms)
        {
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (sector.BoxIndex == boxIndex)
                {
                    count++;
                }
            }
        }
        return count;
    }

    public static int GetSectorCount(TR3Level level, int boxIndex)
    {
        int count = 0;
        foreach (TR3Room room in level.Rooms)
        {
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (sector.BoxIndex == boxIndex)
                {
                    count++;
                }
            }
        }
        return count;
    }
}
