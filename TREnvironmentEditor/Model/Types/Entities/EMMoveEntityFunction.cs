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
            TR2Entity entity = level.Entities[EntityIndex];
            entity.X = TargetLocation.X;
            entity.Y = TargetLocation.Y;
            entity.Z = TargetLocation.Z;
            entity.Room = TargetLocation.Room;
            entity.Angle = TargetLocation.Angle;
        }
    }
}