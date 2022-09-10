using System;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertModelFunction : BaseEMFunction
    {
        public uint OldModelID { get; set; }
        public uint NewModelID { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            ConvertModel(level.Models);
            UpdateModelEntities(level.Entities);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            ConvertModel(level.Models);
            UpdateModelEntities(level.Entities);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            ConvertModel(level.Models);
            UpdateModelEntities(level.Entities);
        }

        private void ConvertModel(TRModel[] models)
        {
            if (Array.Find(models, m => m.ID == NewModelID) == null)
            {
                TRModel oldModel = Array.Find(models, m => m.ID == OldModelID);
                if (oldModel != null)
                {
                    oldModel.ID = NewModelID;
                }
            }
        }

        private void UpdateModelEntities(TREntity[] entities)
        {
            foreach (TREntity entity in entities)
            {
                if (entity.TypeID == OldModelID)
                {
                    entity.TypeID = (short)NewModelID;
                }
            }
        }

        private void UpdateModelEntities(TR2Entity[] entities)
        {
            foreach (TR2Entity entity in entities)
            {
                if (entity.TypeID == OldModelID)
                {
                    entity.TypeID = (short)NewModelID;
                }
            }
        }
    }
}