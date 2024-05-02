namespace TRLevelControl.Model;

public static class FDLevelExtensions
{
    public static TRRoomSector GetRoomSector(this TR1Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR2Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR3Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR4Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR5Level level, int x, int y, int z, short roomNumber)
        => level.FloorData.GetRoomSector(x, y, z, roomNumber, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR1Level level, ITRLocatable location)
        => level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR2Level level, ITRLocatable location)
        => level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR3Level level, ITRLocatable location)
        => level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR4Level level, ITRLocatable location)
        => level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level.Rooms);

    public static TRRoomSector GetRoomSector(this TR5Level level, ITRLocatable location)
        => level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level.Rooms);
}
