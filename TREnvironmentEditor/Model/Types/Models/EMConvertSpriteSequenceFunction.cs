using System;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertSpriteSequenceFunction : BaseEMFunction
    {
        public short OldSpriteID { get; set; }
        public short NewSpriteID { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            ConvertSpriteSequence(level.SpriteSequences);
            UpdateSpriteEntities(level.Entities);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            ConvertSpriteSequence(level.SpriteSequences);
            UpdateSpriteEntities(level.Entities);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            ConvertSpriteSequence(level.SpriteSequences);
            UpdateSpriteEntities(level.Entities);
        }

        private void ConvertSpriteSequence(TRSpriteSequence[] sequences)
        {
            if (Array.Find(sequences, s => s.SpriteID == NewSpriteID) == null)
            {
                TRSpriteSequence oldSequence = Array.Find(sequences, s => s.SpriteID == OldSpriteID);
                if (oldSequence != null)
                {
                    oldSequence.SpriteID = NewSpriteID;
                }
            }
        }

        private void UpdateSpriteEntities(TREntity[] entities)
        {
            foreach (TREntity entity in entities)
            {
                if (entity.TypeID == OldSpriteID)
                {
                    entity.TypeID = NewSpriteID;
                }
            }
        }

        private void UpdateSpriteEntities(TR2Entity[] entities)
        {
            foreach (TR2Entity entity in entities)
            {
                if (entity.TypeID == OldSpriteID)
                {
                    entity.TypeID = NewSpriteID;
                }
            }
        }
    }
}