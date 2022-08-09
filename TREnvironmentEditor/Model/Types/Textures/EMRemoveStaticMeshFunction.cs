using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRemoveStaticMeshFunction : BaseEMFunction
    {
        public EMLocation Location { get; set; }
        public Dictionary<ushort, List<int>> ClearFromRooms { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            if (Location != null)
            {
                TRRoom room = level.Rooms[data.ConvertRoom(Location.Room)];
                List<TRRoomStaticMesh> meshes = room.StaticMeshes.ToList();

                uint x = (uint)Location.X;
                uint y = (uint)(Location.Y < 0 ? uint.MaxValue + Location.Y : Location.Y);
                uint z = (uint)Location.Z;

                TRRoomStaticMesh match = meshes.Find(m => m.X == x && m.Y == y && m.Z == z);
                if (match != null)
                {
                    meshes.Remove(match);
                    room.StaticMeshes = meshes.ToArray();
                    room.NumStaticMeshes--;
                }
            }

            if (ClearFromRooms != null)
            {
                foreach (ushort meshID in ClearFromRooms.Keys)
                {
                    foreach (int roomNumber in ClearFromRooms[meshID])
                    {
                        TRRoom room = level.Rooms[data.ConvertRoom(roomNumber)];
                        List<TRRoomStaticMesh> meshes = room.StaticMeshes.ToList();
                        if (meshes.RemoveAll(m => m.MeshID == meshID) > 0)
                        {
                            room.StaticMeshes = meshes.ToArray();
                            room.NumStaticMeshes = (ushort)meshes.Count;
                        }
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
                List<TR2RoomStaticMesh> meshes = room.StaticMeshes.ToList();

                uint x = (uint)Location.X;
                uint y = (uint)(Location.Y < 0 ? uint.MaxValue + Location.Y : Location.Y);
                uint z = (uint)Location.Z;

                TR2RoomStaticMesh match = meshes.Find(m => m.X == x && m.Y == y && m.Z == z);
                if (match != null)
                {
                    meshes.Remove(match);
                    room.StaticMeshes = meshes.ToArray();
                    room.NumStaticMeshes--;
                }
            }

            if (ClearFromRooms != null)
            {
                foreach (ushort meshID in ClearFromRooms.Keys)
                {
                    foreach (int roomNumber in ClearFromRooms[meshID])
                    {
                        TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                        List<TR2RoomStaticMesh> meshes = room.StaticMeshes.ToList();
                        if (meshes.RemoveAll(m => m.MeshID == meshID) > 0)
                        {
                            room.StaticMeshes = meshes.ToArray();
                            room.NumStaticMeshes = (ushort)meshes.Count;
                        }
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
                List<TR3RoomStaticMesh> meshes = room.StaticMeshes.ToList();

                uint x = (uint)Location.X;
                uint y = (uint)(Location.Y < 0 ? uint.MaxValue + Location.Y : Location.Y);
                uint z = (uint)Location.Z;

                TR3RoomStaticMesh match = meshes.Find(m => m.X == x && m.Y == y && m.Z == z);
                if (match != null)
                {
                    meshes.Remove(match);
                    room.StaticMeshes = meshes.ToArray();
                    room.NumStaticMeshes--;
                }
            }

            if (ClearFromRooms != null)
            {
                foreach (ushort meshID in ClearFromRooms.Keys)
                {
                    foreach (int roomNumber in ClearFromRooms[meshID])
                    {
                        TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                        List<TR3RoomStaticMesh> meshes = room.StaticMeshes.ToList();
                        if (meshes.RemoveAll(m => m.MeshID == meshID) > 0)
                        {
                            room.StaticMeshes = meshes.ToArray();
                            room.NumStaticMeshes = (ushort)meshes.Count;
                        }
                    }
                }
            }
        }
    }
}