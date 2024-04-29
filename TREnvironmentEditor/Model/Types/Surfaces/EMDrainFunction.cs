using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMDrainFunction : BaseWaterFunction
{
    public int[] DetachedRooms { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        
        foreach (int roomNumber in RoomNumbers)
        {
            level.Rooms[data.ConvertRoom(roomNumber)].ContainsWater = false;
        }

        foreach (int roomNumber in RoomNumbers)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(roomNumber)];

            ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
            foreach (byte roomBelowNumber in roomsBelow)
            {
                TR1Room roomBelow = level.Rooms[roomBelowNumber];
                if (roomBelow.ContainsWater)
                {
                    AddWaterSurface(room, false, RoomNumbers);
                    AddWaterSurface(roomBelow, true, RoomNumbers);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR1Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    RemoveWaterSurface(room);
                    RemoveWaterSurface(roomAbove);
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        // Loop initially to drain everything as our texture checks below involve
        // determining which rooms have water.
        foreach (int roomNumber in RoomNumbers)
        {
            level.Rooms[data.ConvertRoom(roomNumber)].ContainsWater = false;
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
                    AddWaterSurface(room, false, RoomNumbers);
                    AddWaterSurface(roomBelow, true, RoomNumbers);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR2Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    RemoveWaterSurface(room);
                    RemoveWaterSurface(roomAbove);
                }
            }
        }

        // Something odd happens with flip map rooms, e.g. 73/134 in Bartoli's.
        // The 2 rooms don't seem to reference each other, so the textures remain,
        // in the flipped room, even though removed from the normal. For now, 
        // we'll define these rooms manually.
        if (DetachedRooms != null)
        {
            foreach (int roomNumber in DetachedRooms)
            {
                RemoveWaterSurface(level.Rooms[data.ConvertRoom(roomNumber)]);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        foreach (int roomNumber in RoomNumbers)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            room.ContainsWater = false;

            // Remove all wave movements and caustics by default
            foreach (TR3RoomVertex vertex in room.Mesh.Vertices)
            {
                vertex.UseWaveMovement = vertex.UseCaustics = false;
            }
        }

        if (WaterTextures == null)
        {
            return;
        }

        FDControl floorData = new();
        floorData.ParseFromLevel(level);

        foreach (int roomNumber in RoomNumbers)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];

            ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
            foreach (byte roomBelowNumber in roomsBelow)
            {
                TR3Room roomBelow = level.Rooms[roomBelowNumber];
                if (roomBelow.ContainsWater)
                {
                    AddWaterSurface(room, false, RoomNumbers, floorData);
                }
            }

            ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
            foreach (byte roomAboveNumber in roomsAbove)
            {
                TR3Room roomAbove = level.Rooms[roomAboveNumber];
                if (!roomAbove.ContainsWater)
                {
                    RemoveWaterSurface(room);
                    RemoveWaterSurface(roomAbove);
                }
            }
        }
    }
}
