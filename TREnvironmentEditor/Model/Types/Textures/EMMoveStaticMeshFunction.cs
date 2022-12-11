using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveStaticMeshFunction : BaseEMFunction
    {
        public Dictionary<short, Dictionary<int, EMLocation>> Relocations { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            foreach (short roomIndex in Relocations.Keys)
            {
                TRRoom room = level.Rooms[data.ConvertRoom(roomIndex)];
                foreach (int meshIndex in Relocations[roomIndex].Keys)
                {
                    TRRoomStaticMesh mesh = room.StaticMeshes[meshIndex];
                    EMLocation location = new EMLocation
                    {
                        X = (int)mesh.X,
                        Y = (int)mesh.Y,
                        Z = (int)mesh.Z,
                    };
                    EMLocation relocation = Relocations[roomIndex][meshIndex];
                    AmendLocation(location, relocation);

                    mesh.X = (uint)location.X;
                    mesh.Y = (uint)location.Y;
                    mesh.Z = (uint)location.Z;
                    mesh.Rotation = (ushort)(relocation.Angle + short.MaxValue + 1);
                }
            }
        }

        private void AmendLocation(EMLocation location, EMLocation amendment)
        {
            location.X += amendment.X;
            location.Y += amendment.Y;
            location.Z += amendment.Z;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            foreach (short roomIndex in Relocations.Keys)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                foreach (int meshIndex in Relocations[roomIndex].Keys)
                {
                    TR2RoomStaticMesh mesh = room.StaticMeshes[meshIndex];
                    EMLocation location = new EMLocation
                    {
                        X = (int)mesh.X,
                        Y = (int)mesh.Y,
                        Z = (int)mesh.Z,
                    };
                    EMLocation relocation = Relocations[roomIndex][meshIndex];
                    AmendLocation(location, relocation);

                    mesh.X = (uint)location.X;
                    mesh.Y = (uint)location.Y;
                    mesh.Z = (uint)location.Z;
                    mesh.Rotation = (ushort)(relocation.Angle + short.MaxValue + 1);
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            foreach (short roomIndex in Relocations.Keys)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                foreach (int meshIndex in Relocations[roomIndex].Keys)
                {
                    TR3RoomStaticMesh mesh = room.StaticMeshes[meshIndex];
                    EMLocation location = new EMLocation
                    {
                        X = (int)mesh.X,
                        Y = (int)mesh.Y,
                        Z = (int)mesh.Z,
                    };
                    EMLocation relocation = Relocations[roomIndex][meshIndex];
                    AmendLocation(location, relocation);

                    mesh.X = (uint)location.X;
                    mesh.Y = (uint)location.Y;
                    mesh.Z = (uint)location.Z;
                    mesh.Rotation = (ushort)(relocation.Angle + short.MaxValue + 1);
                }
            }
        }
    }
}
