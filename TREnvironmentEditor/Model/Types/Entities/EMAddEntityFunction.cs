using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddEntityFunction : BaseEMFunction
    {
        private static readonly int _defaultEntityLimit = 256;

        public short TypeID { get; set; }
        public ushort Flags { get; set; }
        public short? Intensity { get; set; }
        public EMLocation Location { get; set; }
        // If defined, anything else on the same tile will be moved here
        public EMLocation TargetRelocation { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                EMLevelData data = GetData(level);
                if (TargetRelocation != null)
                {
                    FDControl floorData = new FDControl();
                    floorData.ParseFromLevel(level);
                    short room = data.ConvertRoom(Location.Room);
                    TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, room, level, floorData);
                    foreach (TREntity entity in level.Entities)
                    {
                        if (entity.Room == room && FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData) == sector)
                        {
                            entity.X = TargetRelocation.X;
                            entity.Y = TargetRelocation.Y;
                            entity.Z = TargetRelocation.Z;
                            entity.Room = data.ConvertRoom(TargetRelocation.Room);
                        }
                    }
                }

                List<TREntity> entities = level.Entities.ToList();
                entities.Add(CreateTREntity(data));
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                EMLevelData data = GetData(level);
                if (TargetRelocation != null)
                {
                    FDControl floorData = new FDControl();
                    floorData.ParseFromLevel(level);
                    short room = data.ConvertRoom(Location.Room);
                    TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, room, level, floorData);
                    foreach (TR2Entity entity in level.Entities)
                    {
                        if (entity.Room == room && FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData) == sector)
                        {
                            entity.X = TargetRelocation.X;
                            entity.Y = TargetRelocation.Y;
                            entity.Z = TargetRelocation.Z;
                            entity.Room = data.ConvertRoom(TargetRelocation.Room);
                        }
                    }
                }

                List<TR2Entity> entities = level.Entities.ToList();
                entities.Add(CreateTR2Entity(data));
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                EMLevelData data = GetData(level);
                if (TargetRelocation != null)
                {
                    FDControl floorData = new FDControl();
                    floorData.ParseFromLevel(level);
                    short room = data.ConvertRoom(Location.Room);
                    TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, room, level, floorData);
                    foreach (TR2Entity entity in level.Entities)
                    {
                        if (entity.Room == room && FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData) == sector)
                        {
                            entity.X = TargetRelocation.X;
                            entity.Y = TargetRelocation.Y;
                            entity.Z = TargetRelocation.Z;
                            entity.Room = data.ConvertRoom(TargetRelocation.Room);
                        }
                    }
                }

                List<TR2Entity> entities = level.Entities.ToList();
                entities.Add(CreateTR2Entity(data));
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        private TREntity CreateTREntity(EMLevelData data)
        {
            return new TREntity
            {
                TypeID = TypeID,
                X = Location.X,
                Y = Location.Y,
                Z = Location.Z,
                Room = data.ConvertRoom(Location.Room),
                Angle = Location.Angle,
                Flags = Flags,
                Intensity = Intensity ?? 6400
            };
        }

        private TR2Entity CreateTR2Entity(EMLevelData data)
        {
            return new TR2Entity
            {
                TypeID = TypeID,
                X = Location.X,
                Y = Location.Y,
                Z = Location.Z,
                Room = data.ConvertRoom(Location.Room),
                Angle = Location.Angle,
                Flags = Flags,
                Intensity1 = Intensity ?? -1,
                Intensity2 = Intensity ?? -1
            };
        }
    }
}