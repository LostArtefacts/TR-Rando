using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMClickFunction : BaseEMFunction
    {
        // This differs from dedicated floor/ceiling functions by only shifting sector values and does not deal with faces.
        // See example in Masonic room in Aldwych.
        public EMLocation Location { get; set; }
        public sbyte? FloorClicks { get; set; }
        public sbyte? CeilingClicks { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short roomIndex = data.ConvertRoom(Location.Room);
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomIndex, level, floorData);
            MoveSector(sector, level.Rooms[roomIndex].Info);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            if (FloorClicks.HasValue)
            {
                foreach (TREntity entity in level.Entities)
                {
                    if (entity.Room == Location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short roomIndex = data.ConvertRoom(Location.Room);
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomIndex, level, floorData);
            MoveSector(sector, level.Rooms[roomIndex].Info);

            if (FloorClicks.HasValue)
            {
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == Location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            short roomIndex = data.ConvertRoom(Location.Room);
            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, roomIndex, level, floorData);
            MoveSector(sector, level.Rooms[roomIndex].Info);

            if (FloorClicks.HasValue)
            {
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == Location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }

        private void MoveSector(TRRoomSector sector, TRRoomInfo roomInfo)
        {
            if (sector.IsImpenetrable)
            {
                sector.Ceiling = (sbyte)(roomInfo.YTop / 256);
                sector.Floor = (sbyte)(roomInfo.YBottom / 256);
            }
            
            if (FloorClicks.HasValue)
            {
                sector.Floor += FloorClicks.Value;
            }
            if (CeilingClicks.HasValue)
            {
                sector.Ceiling += CeilingClicks.Value;
            }
        }

        protected virtual int GetEntityYShift(int clicks)
        {
            return clicks * 256;
        }
    }
}