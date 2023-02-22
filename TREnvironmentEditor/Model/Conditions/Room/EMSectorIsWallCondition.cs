using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMSectorIsWallCondition : BaseEMCondition
    {
        public EMLocation Location { get; set; }

        protected override bool Evaluate(TRLevel level)
        {
            EMLevelData data = EMLevelData.GetData(level);
            TRRoom room = level.Rooms[data.ConvertRoom(Location.Room)];
            return room.Sectors[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsImpenetrable;
        }
        protected override bool Evaluate(TR2Level level)
        {
            EMLevelData data = EMLevelData.GetData(level);
            TR2Room room = level.Rooms[data.ConvertRoom(Location.Room)];
            return room.SectorList[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsImpenetrable;
        }

        protected override bool Evaluate(TR3Level level)
        {
            EMLevelData data = EMLevelData.GetData(level);
            TR3Room room = level.Rooms[data.ConvertRoom(Location.Room)];
            return room.Sectors[GetSectorIndex(room.Info, Location, room.NumZSectors)].IsImpenetrable;
        }

        private int GetSectorIndex(TRRoomInfo roomInfo, EMLocation location, int roomDepth)
        {
            int x = (location.X - roomInfo.X) / 1024;
            int z = (location.Z - roomInfo.Z) / 1024;
            return x * roomDepth + z;
        }
    }
}
