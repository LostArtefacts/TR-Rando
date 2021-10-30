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
            MoveEntity(level.Entities[EntityIndex]);
            
        }

        public override void ApplyToLevel(TR3Level level)
        {
            MoveEntity(level.Entities[EntityIndex]);
        }

        private void MoveEntity(TR2Entity entity)
        {
            entity.X = TargetLocation.X;
            entity.Y = TargetLocation.Y;
            entity.Z = TargetLocation.Z;
            entity.Room = TargetLocation.Room;
            entity.Angle = TargetLocation.Angle;
        }
    }
}