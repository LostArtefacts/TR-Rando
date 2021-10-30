using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddStaticMeshFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public TR2RoomStaticMesh Mesh { get; set; }
        public bool IgnoreSectorEntities { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                TR2Room room = level.Rooms[location.Room];

                // Only add this mesh if there is nothing else in the same sector.
                bool sectorFree = true;
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                        if (entitySector == sector)
                        {
                            sectorFree = false;
                            break;
                        }
                    }
                }

                if (!sectorFree && !IgnoreSectorEntities)
                {
                    continue;
                }

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

        public override void ApplyToLevel(TR3Level level)
        {
            throw new System.NotImplementedException();
        }
    }
}