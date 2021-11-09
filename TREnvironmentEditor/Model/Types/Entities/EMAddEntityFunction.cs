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
        public EMLocation Location { get; set; }

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
                Intensity1 = -1,
                Intensity2 = -1
            };
        }
    }
}