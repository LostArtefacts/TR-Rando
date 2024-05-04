using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMFloodFunction : BaseWaterFunction
{
    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        // Loop initially to flood everything as our texture checks below involve
        // determining which rooms have water.
        foreach (int roomNumber in RoomNumbers)
        {
            level.Rooms[data.ConvertRoom(roomNumber)].ContainsWater = true;
        }

        // Work out rooms above and below and what needs water textures as ceilings
        // and what needs them as floors.
        foreach (int roomNumber in RoomNumbers)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(roomNumber)];

            ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
            foreach (byte roomBelowNumber in roomsBelow)
            {
                TR1Room roomBelow = level.Rooms[roomBelowNumber];
                if (roomBelow.ContainsWater)
                {
                    RemoveWaterSurface(room);
                    RemoveWaterSurface(roomBelow);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR1Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    AddWaterSurface(room, true, new int[] { roomAboveNumber });
                    AddWaterSurface(roomAbove, false, RoomNumbers);
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        // Loop initially to flood everything as our texture checks below involve
        // determining which rooms have water.
        foreach (int roomNumber in RoomNumbers)
        {
            level.Rooms[data.ConvertRoom(roomNumber)].ContainsWater = true;
        }

        // Work out rooms above and below and what needs water textures as ceilings
        // and what needs them as floors.
        foreach (int roomNumber in RoomNumbers)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];

            ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
            foreach (byte roomBelowNumber in roomsBelow)
            {
                TR2Room roomBelow = level.Rooms[roomBelowNumber];
                if (roomBelow.ContainsWater)
                {
                    RemoveWaterSurface(room);
                    RemoveWaterSurface(roomBelow);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR2Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    AddWaterSurface(room, true, new int[] { roomAboveNumber });
                    AddWaterSurface(roomAbove, false, RoomNumbers);
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (int roomNumber in RoomNumbers)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            room.ContainsWater = true;
            foreach (TR3RoomVertex vertex in room.Mesh.Vertices)
            {
                vertex.UseCaustics = true;
            }
        }

        foreach (int roomNumber in RoomNumbers)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];

            ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
            foreach (byte roomBelowNumber in roomsBelow)
            {
                TR3Room roomBelow = level.Rooms[roomBelowNumber];
                if (roomBelow.ContainsWater)
                {
                    RemoveWaterSurface(room);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR3Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    AddWaterSurface(room, true, new int[] { roomAboveNumber }, level.FloorData);
                    AddWaterSurface(roomAbove, false, RoomNumbers, level.FloorData);
                }
            }
        }
    }
}
