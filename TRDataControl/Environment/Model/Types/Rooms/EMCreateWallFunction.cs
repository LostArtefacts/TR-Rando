using TREnvironmentEditor.Helpers;
using TRLevelControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCreateWallFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public EMLocation EntityMoveLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TRRoomSector sector = level.GetRoomSector(position);
            BlockSector(sector);

            // Move any entities that share the same floor sector somewhere else
            if (EntityMoveLocation == null)
            {
                continue;
            }

            foreach (TR1Entity entity in level.Entities.Where(e => e.Room == position.Room))
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.X = EntityMoveLocation.X;
                    entity.Y = EntityMoveLocation.Y;
                    entity.Z = EntityMoveLocation.Z;
                    entity.Room += data.ConvertRoom(EntityMoveLocation.Room);
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TRRoomSector sector = level.GetRoomSector(position);
            BlockSector(sector);

            // Move any entities that share the same floor sector somewhere else
            if (EntityMoveLocation == null)
            {
                continue;
            }

            foreach (TR2Entity entity in level.Entities.Where(e => e.Room == position.Room))
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.X = EntityMoveLocation.X;
                    entity.Y = EntityMoveLocation.Y;
                    entity.Z = EntityMoveLocation.Z;
                    entity.Room += data.ConvertRoom(EntityMoveLocation.Room);
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            EMLocation position = data.ConvertLocation(location);
            TRRoomSector sector = level.GetRoomSector(position);
            BlockSector(sector);

            // Move any entities that share the same floor sector somewhere else
            if (EntityMoveLocation == null)
            {
                continue;
            }

            foreach (TR3Entity entity in level.Entities.Where(e => e.Room == position.Room))
            {
                TRRoomSector entitySector = level.GetRoomSector(entity);
                if (entitySector == sector)
                {
                    entity.X = EntityMoveLocation.X;
                    entity.Y = EntityMoveLocation.Y;
                    entity.Z = EntityMoveLocation.Z;
                    entity.Room += data.ConvertRoom(EntityMoveLocation.Room);
                }
            }
        }
    }

    private static void BlockSector(TRRoomSector sector)
    {
        sector.Floor = sector.Ceiling = TRConsts.WallClicks;
        sector.BoxIndex = TRConsts.NoBox;
    }
}
