using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertEntityFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public short NewEntityType { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            level.Entities[data.ConvertEntity(EntityIndex)].TypeID = NewEntityType;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            level.Entities[data.ConvertEntity(EntityIndex)].TypeID = NewEntityType;
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            level.Entities[data.ConvertEntity(EntityIndex)].TypeID = NewEntityType;
        }
    }
}