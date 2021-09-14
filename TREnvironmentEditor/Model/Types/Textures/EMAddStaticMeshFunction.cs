using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddStaticMeshFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public TR2RoomStaticMesh Mesh { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            foreach (EMLocation location in Locations)
            {
                TR2Room room = level.Rooms[location.Room];
                List<TR2RoomStaticMesh> meshes = room.StaticMeshes.ToList();

                meshes.Add(new TR2RoomStaticMesh
                {
                    X = (uint)location.X,
                    Y = (uint)(location.Y < 0 ? uint.MaxValue + location.Y : location.Y),
                    Z = (uint)location.Z,
                    Intensity1 = Mesh.Intensity1,
                    Intensity2 = Mesh.Intensity2,
                    MeshID = Mesh.MeshID,
                    Rotation = (ushort)(location.Angle + short.MaxValue + 1)
                });

                room.StaticMeshes = meshes.ToArray();
                room.NumStaticMeshes = (ushort)meshes.Count;
            }
        }
    }
}