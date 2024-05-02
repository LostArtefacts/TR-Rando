namespace TRLevelControl.Model;

public static class FDLevelExtensions
{
    public static TRRoomSector GetRoomSector(this TR1Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR2Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR3Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);
}
