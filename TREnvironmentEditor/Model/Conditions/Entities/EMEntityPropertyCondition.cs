using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMEntityPropertyCondition : BaseEMCondition
    {
        public int EntityIndex { get; set; }
        public short? EntityType { get; set; }
        public bool? Invisible { get; set; }
        public bool? ClearBody { get; set; }
        public short? Intensity1 { get; set; }
        public short? Intensity2 { get; set; }
        public ushort? Flags { get; set; }

        public override bool GetResult(TR2Level level)
        {
            return GetResult(level.Entities[EntityIndex]);
        }

        public override bool GetResult(TR3Level level)
        {
            return GetResult(level.Entities[EntityIndex]);
        }

        private bool GetResult(TR2Entity entity)
        {
            bool result = true;
            if (EntityType.HasValue)
            {
                result &= entity.TypeID == EntityType.Value;
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