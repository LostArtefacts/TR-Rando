using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveEntityFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public EMLocation TargetLocation { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            MoveEntity(level.Entities[EntityIndex], level.NumRooms);
            
        }

        public override void ApplyToLevel(TR3Level level)
        {
            MoveEntity(level.Entities[EntityIndex], level.NumRooms);
        }

        private void MoveEntity(TR2Entity entity, ushort numRooms)
        {
            entity.X = TargetLocation.X;
            entity.Y = TargetLocation.Y;
            entity.Z = TargetLocation.Z;
            entity.Room = (short)ConvertItemNumber(TargetLocation.Room, numRooms);
            entity.Angle = TargetLocation.Angle;
        }
    }
}