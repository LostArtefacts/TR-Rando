using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveSecretFunction : EMMovePickupFunction
    {
        public override void ApplyToLevel(TRLevel level)
        {
            Types = new List<short>();
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            foreach (EMLocation location in SectorLocations)
            {
                int secretIndex = location.GetContainedSecret(level, floorData);
                if (secretIndex != -1)
                {
                    Types.Add(level.Entities[secretIndex].TypeID);
                }
            }

            base.ApplyToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            Types = new List<short>();
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            foreach (EMLocation location in SectorLocations)
            {
                int secretIndex = location.GetContainedSecret(level, floorData);
                if (secretIndex != -1)
                {
                    Types.Add(level.Entities[secretIndex].TypeID);
                }
            }

            base.ApplyToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            Types = new List<short>();
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);
            foreach (EMLocation location in SectorLocations)
            {
                int secretIndex = location.GetContainedSecret(level, floorData);
                if (secretIndex != -1)
                {
                    Types.Add(level.Entities[secretIndex].TypeID);
                }
            }

            base.ApplyToLevel(level);
        }
    }
}
