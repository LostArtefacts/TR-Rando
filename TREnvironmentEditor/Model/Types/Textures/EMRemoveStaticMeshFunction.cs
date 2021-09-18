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

        public override void ApplyToLevel(TR2Level level)
        {
            if (Location != null)
            {
                TR2Room room = level.Rooms[Location.Room];
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
                        TR2Room room = level.Rooms[roomNumber];
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
    }
}