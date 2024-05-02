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
        short ceilingRoom = data.ConvertRoom(Ceiling.Room);
        short floorRoom = data.ConvertRoom(Floor.Room);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom;
                floorSector.RoomAbove = (byte)ceilingRoom;

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
        short ceilingRoom = data.ConvertRoom(Ceiling.Room);
        short floorRoom = data.ConvertRoom(Floor.Room);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom;
                floorSector.RoomAbove = (byte)ceilingRoom;

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
        short ceilingRoom = data.ConvertRoom(Ceiling.Room);
        short floorRoom = data.ConvertRoom(Floor.Room);

        if (AllSectors)
        {
            foreach (TRRoomSector sector in level.Rooms[ceilingRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomBelow != TRConsts.NoRoom)
                {
                    sector.RoomBelow = (byte)floorRoom;
                }
            }

            foreach (TRRoomSector sector in level.Rooms[floorRoom].Sectors)
            {
                if (!sector.IsWall && sector.RoomAbove != TRConsts.NoRoom)
                {
                    sector.RoomAbove = (byte)ceilingRoom;
                }
            }
        }
        else
        {
            TRRoomSector ceilingSector = level.GetRoomSector(Ceiling.X, Ceiling.Y, Ceiling.Z, ceilingRoom);
            TRRoomSector floorSector = level.GetRoomSector(Floor.X, Floor.Y, Floor.Z, floorRoom);

            if (ceilingSector != floorSector)
            {
                ceilingSector.RoomBelow = (byte)floorRoom;
                floorSector.RoomAbove = (byte)ceilingRoom;

                if (InheritFloorBox)
                {
                    ceilingSector.BoxIndex = floorSector.BoxIndex;
                }
            }
        }
    }
}
