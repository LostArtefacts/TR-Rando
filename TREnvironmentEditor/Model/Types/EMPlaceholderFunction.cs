using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMPlaceholderFunction : BaseEMFunction
    {
        // NOOP

        public override void ApplyToLevel(TRLevel level) { }

        public override void ApplyToLevel(TR2Level level) { }

        public override void ApplyToLevel(TR3Level level) { }
    }
}