using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMModifyEntityFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public bool? Invisible { get; set; }
        public bool? ClearBody { get; set; }
        public short? Intensity1 { get; set; }
        public short? Intensity2 { get; set; }
        public ushort? Flags { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            ModifyEntity(level.Entities[data.ConvertEntity(EntityIndex)]);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            ModifyEntity(level.Entities[data.ConvertEntity(EntityIndex)]);            
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            ModifyEntity(level.Entities[data.ConvertEntity(EntityIndex)]);
        }

        private void ModifyEntity(TREntity entity)
        {
            if (Invisible.HasValue)
            {
                entity.Invisible = Invisible.Value;
            }
            if (ClearBody.HasValue)
            {
                entity.ClearBody = ClearBody.Value;
            }
            if (Intensity1.HasValue)
            {
                entity.Intensity = Intensity1.Value;
            }
            if (Intensity2.HasValue)
            {
                entity.Intensity = Intensity2.Value;
            }
            if (Flags.HasValue)
            {
                entity.Flags = Flags.Value;
            }
        }

        private void ModifyEntity(TR2Entity entity)
        {
            if (Invisible.HasValue)
            {
                entity.Invisible = Invisible.Value;
            }
            if (ClearBody.HasValue)
            {
                entity.ClearBody = ClearBody.Value;
            }
            if (Intensity1.HasValue)
            {
                entity.Intensity1 = Intensity1.Value;
            }
            if (Intensity2.HasValue)
            {
                entity.Intensity2 = Intensity2.Value;
            }
            if (Flags.HasValue)
            {
                entity.Flags = Flags.Value;
            }
        }
    }
}