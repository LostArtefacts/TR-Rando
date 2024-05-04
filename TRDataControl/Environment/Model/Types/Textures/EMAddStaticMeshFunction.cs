using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddStaticMeshFunction : BaseEMFunction
{
    public uint MeshID { get; set; }
    public ushort Intensity { get; set; }
    public List<EMLocation> Locations { get; set; }
    public bool IgnoreSectorEntities { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TR1Room room = level.Rooms[position.Room];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = level.GetRoomSector(position);
                foreach (TR1Entity entity in level.Entities)
                {
                    if (entity.Room == position.Room)
                    {
                        TRRoomSector entitySector = level.GetRoomSector(entity);
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
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Intensity = Intensity,
                ID = (TR1Type)MeshID,
                Angle = position.Angle
            });
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TR2Room room = level.Rooms[position.Room];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = level.GetRoomSector(position);
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == position.Room)
                    {
                        TRRoomSector entitySector = level.GetRoomSector(entity);
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
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Intensity1 = Intensity,
                Intensity2 = Intensity,
                ID = (TR2Type)MeshID,
                Angle = position.Angle
            });
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TR3Room room = level.Rooms[position.Room];

            // Only add this mesh if there is nothing else in the same sector.
            if (!IgnoreSectorEntities)
            {
                bool sectorFree = true;
                TRRoomSector sector = level.GetRoomSector(position);
                foreach (TR3Entity entity in level.Entities)
                {
                    if (entity.Room == position.Room)
                    {
                        TRRoomSector entitySector = level.GetRoomSector(entity);
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
                X = position.X,
                Y = position.Y,
                Z = position.Z,
                Colour = Intensity,
                ID = (TR3Type)MeshID,
                Angle = position.Angle
            });
        }
    }
}
