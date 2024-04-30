using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddStaticMeshFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public TR2RoomStaticMesh Mesh { get; set; }
    public bool IgnoreSectorEntities { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        foreach (EMLocation location in Locations)
        {
            short roomNumber = data.ConvertRoom(location.Room);
            TR1Room room = level.Rooms[roomNumber];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomNumber, level, control);
                foreach (TR1Entity entity in level.Entities)
                {
                    if (entity.Room == roomNumber)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                        if (entitySector == sector)
                        {
                            sectorFree = false;
                            break;
                        }
                    }
                }

                if (!sectorFree)
                {
                    continue;
                }
            }

            room.StaticMeshes.Add(new()
            {
                X = location.X,
                Y = location.Y,
                Z = location.Z,
                Intensity = Mesh.Intensity1,
                ID = Mesh.ID,
                Rotation = location.Angle
            });
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        foreach (EMLocation location in Locations)
        {
            short roomNumber = data.ConvertRoom(location.Room);
            TR2Room room = level.Rooms[roomNumber];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomNumber, level, control);
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == roomNumber)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                        if (entitySector == sector)
                        {
                            sectorFree = false;
                            break;
                        }
                    }
                }

                if (!sectorFree)
                {
                    continue;
                }
            }

            room.StaticMeshes.Add(new()
            {
                X = location.X,
                Y = location.Y,
                Z = location.Z,
                Intensity1 = Mesh.Intensity1,
                Intensity2 = Mesh.Intensity2,
                ID = Mesh.ID,
                Rotation = location.Angle
            });
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        FDControl control = new();
        control.ParseFromLevel(level);

        foreach (EMLocation location in Locations)
        {
            short roomNumber = data.ConvertRoom(location.Room);
            TR3Room room = level.Rooms[roomNumber];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomNumber, level, control);
                foreach (TR3Entity entity in level.Entities)
                {
                    if (entity.Room == roomNumber)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                        if (entitySector == sector)
                        {
                            sectorFree = false;
                            break;
                        }
                    }
                }

                if (!sectorFree)
                {
                    continue;
                }
            }

            room.StaticMeshes.Add(new()
            {
                X = location.X,
                Y = location.Y,
                Z = location.Z,
                Colour = Mesh.Intensity1,
                ID = Mesh.ID,
                Rotation = location.Angle
            });
        }
    }
}
