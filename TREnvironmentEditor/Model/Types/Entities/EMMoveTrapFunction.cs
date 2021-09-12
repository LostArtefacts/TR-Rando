using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveTrapFunction : BaseMoveTriggerableFunction
    {
        public override void ApplyToLevel(TR2Level level)
        {
            TR2Entity trap = level.Entities[EntityIndex];
            RepositionTriggerable(trap, level);
        }
    }
}