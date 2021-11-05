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

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short ceilingRoom = (short)ConvertItemNumber(Ceiling.Room, level.NumRooms);
            short floorRoom = (short)ConvertItemNumber(Floor.Room, level.NumRooms);

            TRRoomSector ceilingSector = FDUtilities.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom, level, floorData);
            TRRoomSector floorSector = FDUtilities.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom, level, floorData);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom;
                floorSector.RoomAbove = (byte)ceilingRoom;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short ceilingRoom = (short)ConvertItemNumber(Ceiling.Room, level.NumRooms);
            short floorRoom = (short)ConvertItemNumber(Floor.Room, level.NumRooms);

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