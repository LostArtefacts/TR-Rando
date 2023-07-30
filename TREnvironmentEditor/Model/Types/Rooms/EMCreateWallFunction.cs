using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMCreateWallFunction : BaseEMFunction
    {
        public List<EMLocation> Locations { get; set; }
        public EMLocation EntityMoveLocation { get; set; }

        public override void ApplyToLevel(TR1Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                short roomIndex = data.ConvertRoom(location.Room);
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomIndex, level, floorData);
                BlockSector(sector, floorData);

                // Move any entities that share the same floor sector somewhere else
                if (EntityMoveLocation != null)
                {
                    foreach (TREntity entity in level.Entities)
                    {
                        if (entity.Room == roomIndex)
                        {
                            TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
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
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                short roomIndex = data.ConvertRoom(location.Room);
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomIndex, level, floorData);
                BlockSector(sector, floorData);

                // Move any entities that share the same floor sector somewhere else
                if (EntityMoveLocation != null)
                {
                    foreach (TR2Entity entity in level.Entities)
                    {
                        if (entity.Room == roomIndex)
                        {
                            TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
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
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in Locations)
            {
                short roomIndex = data.ConvertRoom(location.Room);
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, roomIndex, level, floorData);
                BlockSector(sector, floorData);

                // Move any entities that share the same floor sector somewhere else
                if (EntityMoveLocation != null)
                {
                    foreach (TR2Entity entity in level.Entities)
                    {
                        if (entity.Room == roomIndex)
                        {
                            TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
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
            }

            floorData.WriteToLevel(level);
        }

        private void BlockSector(TRRoomSector sector, FDControl floorData)
        {
            sector.Floor = sector.Ceiling = -127;
            sector.BoxIndex = ushort.MaxValue;
            if (sector.FDIndex != 0)
            {
                floorData.RemoveFloorData(sector);
            }
        }
    }
}
