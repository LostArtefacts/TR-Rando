using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public abstract class BaseEMCondition
    {
        public EMConditionType ConditionType { get; set; }

        public abstract bool GetResult(TR2Level level);
    }
}