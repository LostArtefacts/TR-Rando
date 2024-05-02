using TREnvironmentEditor.Helpers;
using TRLevelControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMVerticalCollisionalPortalFunction : BaseEMFunction
{
    public EMLocation Ceiling { get; set; }
    public EMLocation Floor { get; set; }
    public bool AllSectors { get; set; }
    public bool InheritFloorBox { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        EMLocation ceilingRoom = data.ConvertLocation(Ceiling);
        EMLocation floorRoom = data.ConvertLocation(Floor);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom.Room;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom.Room;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom.Room;
                floorSector.RoomAbove = (byte)ceilingRoom.Room;

                if (InheritFloorBox)
                {
                    ceilingSector.BoxIndex = floorSector.BoxIndex;
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        EMLocation ceilingRoom = data.ConvertLocation(Ceiling);
        EMLocation floorRoom = data.ConvertLocation(Floor);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom.Room;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom.Room;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom.Room;
                floorSector.RoomAbove = (byte)ceilingRoom.Room;

                if (InheritFloorBox)
                {
                    ceilingSector.BoxIndex = floorSector.BoxIndex;
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        EMLocation ceilingRoom = data.ConvertLocation(Ceiling);
        EMLocation floorRoom = data.ConvertLocation(Floor);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom.Room;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom.Room].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom.Room;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom.Room;
                floorSector.RoomAbove = (byte)ceilingRoom.Room;

                if (InheritFloorBox)
                {
                    ceilingSector.BoxIndex = floorSector.BoxIndex;
                }
            }
        }
    }
}
