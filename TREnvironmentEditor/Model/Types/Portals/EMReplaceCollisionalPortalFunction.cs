using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMReplaceCollisionalPortalFunction : BaseEMFunction
    {
        public short Room { get; set; }
        public short X { get; set; }
        public short Z { get; set; }
        public short AdjoiningRoom { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoom room = level.Rooms[data.ConvertRoom(Room)];
            TRRoomSector sector = room.Sectors[X * room.NumZSectors + Z];
            ReplacePortal(sector, (ushort)data.ConvertRoom(AdjoiningRoom), floorData);

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TR2Room room = level.Rooms[data.ConvertRoom(Room)];
            TRRoomSector sector = room.SectorList[X * room.NumZSectors + Z];
            ReplacePortal(sector, (ushort)data.ConvertRoom(AdjoiningRoom), floorData);

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TR3Room room = level.Rooms[data.ConvertRoom(Room)];
            TRRoomSector sector = room.Sectors[X * room.NumZSectors + Z];
            ReplacePortal(sector, (ushort)data.ConvertRoom(AdjoiningRoom), floorData);

            floorData.WriteToLevel(level);
        }

        private void ReplacePortal(TRRoomSector sector, ushort adjoiningRoom, FDControl floorData)
        {
            if (sector.FDIndex == 0)
            {
                return;
            }

            foreach (FDEntry entry in floorData.Entries[sector.FDIndex].FindAll(e => e is FDPortalEntry))
            {
                (entry as FDPortalEntry).Room = adjoiningRoom;
            }
        }
    }
}
