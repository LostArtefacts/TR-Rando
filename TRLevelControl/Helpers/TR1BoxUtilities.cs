using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public static class TR1BoxUtilities
{
    public static int GetSectorCount(TR1Level level, int boxIndex)
    {
        int count = 0;
        foreach (TR1Room room in level.Rooms)
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
