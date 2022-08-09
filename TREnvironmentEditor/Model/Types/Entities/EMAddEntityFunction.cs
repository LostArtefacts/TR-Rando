using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddEntityFunction : BaseEMFunction
    {
        private static readonly int _defaultEntityLimit = 256;

        public short TypeID { get; set; }
        public ushort Flags { get; set; }
        public short? Intensity { get; set; }
        public EMLocation Location { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                List<TREntity> entities = level.Entities.ToList();
                entities.Add(CreateTREntity());
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                List<TR2Entity> entities = level.Entities.ToList();
                entities.Add(CreateTR2Entity());
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            if (level.NumEntities < _defaultEntityLimit)
            {
                List<TR2Entity> entities = level.Entities.ToList();
                entities.Add(CreateTR2Entity());
                level.Entities = entities.ToArray();
                level.NumEntities++;
            }
        }

        private TREntity CreateTREntity()
        {
            return new TREntity
            {
                TypeID = TypeID,
                X = Location.X,
                Y = Location.Y,
                Z = Location.Z,
                Room = Location.Room,
                Angle = Location.Angle,
                Flags = Flags,
                Intensity = Intensity ?? 6400
            };
        }

        private TR2Entity CreateTR2Entity()
        {
            return new TR2Entity
            {
                TypeID = TypeID,
                X = Location.X,
                Y = Location.Y,
                Z = Location.Z,
                Room = Location.Room,
                Angle = Location.Angle,
                Flags = Flags,
                Intensity1 = Intensity ?? -1,
                Intensity2 = Intensity ?? -1
            };
        }
    }
}