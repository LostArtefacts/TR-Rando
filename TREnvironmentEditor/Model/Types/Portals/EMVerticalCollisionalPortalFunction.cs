using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMVerticalCollisionalPortalFunction : BaseEMFunction
    {
        public EMLocation Ceiling { get; set; }
        public EMLocation Floor { get; set; }
        public bool AllSectors { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short ceilingRoom = data.ConvertRoom(Ceiling.Room);
            short floorRoom = data.ConvertRoom(Floor.Room);

            if (AllSectors)
            {
                foreach (TRRoomSector sector in level.Rooms[ceilingRoom].Sectors)
                {
                    if (!sector.IsImpenetrable && sector.RoomBelow != 255)
                    {
                        sector.RoomBelow = (byte)floorRoom;
                    }
                }

                foreach (TRRoomSector sector in level.Rooms[floorRoom].Sectors)
                {
                    if (!sector.IsImpenetrable && sector.RoomAbove != 255)
                    {
                        sector.RoomAbove = (byte)ceilingRoom;
                    }
                }
            }
            else
            {
                TRRoomSector ceilingSector = FDUtilities.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom, level, floorData);
                TRRoomSector floorSector = FDUtilities.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom, level, floorData);

                if (ceilingSector != floorSector)
                {
                    ceilingSector.RoomBelow = (byte)floorRoom;
                    floorSector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short ceilingRoom = data.ConvertRoom(Ceiling.Room);
            short floorRoom = data.ConvertRoom(Floor.Room);

            if (AllSectors)
            {
                foreach (TRRoomSector sector in level.Rooms[ceilingRoom].SectorList)
                {
                    if (!sector.IsImpenetrable && sector.RoomBelow != 255)
                    {
                        sector.RoomBelow = (byte)floorRoom;
                    }
                }

                foreach (TRRoomSector sector in level.Rooms[floorRoom].SectorList)
                {
                    if (!sector.IsImpenetrable && sector.RoomAbove != 255)
                    {
                        sector.RoomAbove = (byte)ceilingRoom;
                    }
                }
            }
            else
            {
                TRRoomSector ceilingSector = FDUtilities.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom, level, floorData);
                TRRoomSector floorSector = FDUtilities.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom, level, floorData);

                if (ceilingSector != floorSector)
                {
                    ceilingSector.RoomBelow = (byte)floorRoom;
                    floorSector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short ceilingRoom = data.ConvertRoom(Ceiling.Room);
            short floorRoom = data.ConvertRoom(Floor.Room);

            if (AllSectors)
            {
                foreach (TRRoomSector sector in level.Rooms[ceilingRoom].Sectors)
                {
                    if (!sector.IsImpenetrable && sector.RoomBelow != 255)
                    {
                        sector.RoomBelow = (byte)floorRoom;
                    }
                }

                foreach (TRRoomSector sector in level.Rooms[floorRoom].Sectors)
                {
                    if (!sector.IsImpenetrable && sector.RoomAbove != 255)
                    {
                        sector.RoomAbove = (byte)ceilingRoom;
                    }
                }
            }
            else
            {
                TRRoomSector ceilingSector = FDUtilities.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom, level, floorData);
                TRRoomSector floorSector = FDUtilities.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom, level, floorData);

                if (ceilingSector != floorSector)
                {
                    ceilingSector.RoomBelow = (byte)floorRoom;
                    floorSector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }
    }
}