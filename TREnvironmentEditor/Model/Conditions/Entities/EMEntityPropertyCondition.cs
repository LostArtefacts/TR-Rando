using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMEntityPropertyCondition : BaseEMCondition
    {
        public int EntityIndex { get; set; }
        public TR2Entities? EntityType { get; set; }
        public bool? Invisible { get; set; }
        public bool? ClearBody { get; set; }
        public short? Intensity1 { get; set; }
        public short? Intensity2 { get; set; }
        public ushort? Flags { get; set; }

        public override bool GetResult(TR2Level level)
        {
            TR2Entity entity = level.Entities[EntityIndex];
            bool result = true;
            if (EntityType.HasValue)
            {
                result &= (TR2Entities)entity.TypeID == EntityType.Value;
            }
            if (Invisible.HasValue)
            {
                result &= entity.Invisible == Invisible.Value;
            }
            if (ClearBody.HasValue)
            {
                result &= entity.ClearBody == ClearBody.Value;
            }
            if (Intensity1.HasValue)
            {
                result &= entity.Intensity1 == Intensity1.Value;
            }
            if (Intensity2.HasValue)
            {
                result &= entity.Intensity2 == Intensity2.Value;
            }
            if (Flags.HasValue)
            {
                result &= entity.Flags == Flags.Value;
            }
            return result;
        }
    }
}