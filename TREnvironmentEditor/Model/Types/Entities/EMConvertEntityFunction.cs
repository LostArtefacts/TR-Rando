using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertEntityFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public short NewEntityType { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            level.Entities[EntityIndex].TypeID = NewEntityType;
        }

        public override void ApplyToLevel(TR3Level level)
        {
            level.Entities[EntityIndex].TypeID = NewEntityType;
        }
    }
}