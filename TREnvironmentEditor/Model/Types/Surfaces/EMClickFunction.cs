using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMClickFunction : BaseEMFunction
    {
        // This differs from dedicated floor/ceiling functions by only shifting sector values and does not deal with faces.
        // See example in Masonic room in Aldwych.
        public EMLocation Location { get; set; }
        public sbyte? FloorClicks { get; set; }
        public sbyte? CeilingClicks { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, floorData);
            MoveSector(sector);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, floorData);
            MoveSector(sector);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, data.ConvertRoom(Location.Room), level, floorData);
            MoveSector(sector);
        }

        private void MoveSector(TRRoomSector sector)
        {
            if (FloorClicks.HasValue)
            {
                sector.Floor += FloorClicks.Value;
            }
            if (CeilingClicks.HasValue)
            {
                sector.Ceiling += CeilingClicks.Value;
            }
        }
    }
}