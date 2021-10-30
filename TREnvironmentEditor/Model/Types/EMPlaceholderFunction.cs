using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMPlaceholderFunction : BaseEMFunction
    {
        public override void ApplyToLevel(TR2Level level)
        {
            // NOOP
        }

        public override void ApplyToLevel(TR3Level level) { }
    }
}