using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveTrapFunction : BaseMoveTriggerableFunction
    {
        public override void ApplyToLevel(TRLevel level)
        {
            TREntity trap = level.Entities[EntityIndex];
            RepositionTriggerable(trap, level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            TR2Entity trap = level.Entities[EntityIndex];
            RepositionTriggerable(trap, level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            TR2Entity trap = level.Entities[EntityIndex];
            RepositionTriggerable(trap, level);
        }
    }
}