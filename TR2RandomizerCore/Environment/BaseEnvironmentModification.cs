using TRLevelReader.Model;

namespace TR2RandomizerCore.Environment
{
    public abstract class BaseEnvironmentModification
    {
        public bool Enforced { get; set; }

        public abstract void ApplyToLevel(TR2Level level);
    }
}