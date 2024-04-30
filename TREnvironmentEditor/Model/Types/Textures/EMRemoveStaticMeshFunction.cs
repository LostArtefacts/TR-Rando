using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMRemoveStaticMeshFunction : BaseEMFunction
{
    public EMLocation Location { get; set; }
    public Dictionary<uint, List<int>> ClearFromRooms { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        if (Location != null)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(Location.Room)];
            room.StaticMeshes.RemoveAll(m => m.X == Location.X && m.Y == Location.Y && m.Z == Location.Z);
        }

        if (ClearFromRooms != null)
        {
            foreach (var (meshID, roomList) in ClearFromRooms)
            {
                foreach (int roomNumber in roomList)
                {
                    TR1Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                    room.StaticMeshes.RemoveAll(m => m.ID == (TR1Type)meshID);
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        if (Location != null)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(Location.Room)];
            room.StaticMeshes.RemoveAll(m => m.X == Location.X && m.Y == Location.Y && m.Z == Location.Z);
        }

        if (ClearFromRooms != null)
        {
            foreach (var (meshID, roomList) in ClearFromRooms)
            {
                foreach (int roomNumber in roomList)
                {
                    TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                    room.StaticMeshes.RemoveAll(m => m.ID == (TR2Type)meshID);
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        if (Location != null)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(Location.Room)];
            room.StaticMeshes.RemoveAll(m => m.X == Location.X && m.Y == Location.Y && m.Z == Location.Z);
        }

        if (ClearFromRooms != null)
        {
            foreach (var (meshID, roomList) in ClearFromRooms)
            {
                foreach (int roomNumber in roomList)
                {
                    TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                    room.StaticMeshes.RemoveAll(m => m.ID == (TR3Type)meshID);
                }
            }
        }
    }
}
