using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAdjustEntityPositionFunction : BaseEMFunction
    {
        public short EntityType { get; set; }
        public Dictionary<int, Dictionary<short, EMLocation>> RoomMap { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            List<TREntity> entities = level.Entities.ToList().FindAll(e => e.TypeID == EntityType);
            AdjustEntities(entities);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            // Example use case is rotating wall blades, which need various different angles across the levels after mirroring.
            // X, Y, Z in the target relocation will be relative to the current location; the angle will be the new angle.

            List<TR2Entity> entities = level.Entities.ToList().FindAll(e => e.TypeID == EntityType);
            AdjustEntities(entities);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TR2Entity> entities = level.Entities.ToList().FindAll(e => e.TypeID == EntityType);
            AdjustEntities(entities);
        }

        private void AdjustEntities(List<TREntity> entities)
        {
            foreach (int roomNumber in RoomMap.Keys)
            {
                foreach (short currentAngle in RoomMap[roomNumber].Keys)
                {
                    EMLocation relocation = RoomMap[roomNumber][currentAngle];
                    List<TREntity> matchingEntities = entities.FindAll(e => e.Room == roomNumber && e.Angle == currentAngle);
                    foreach (TREntity match in matchingEntities)
                    {
                        match.X += relocation.X;
                        match.Y += relocation.Y;
                        match.Z += relocation.Z;
                        match.Angle = relocation.Angle;
                    }
                }
            }
        }

        private void AdjustEntities(List<TR2Entity> entities)
        {
            foreach (int roomNumber in RoomMap.Keys)
            {
                foreach (short currentAngle in RoomMap[roomNumber].Keys)
                {
                    EMLocation relocation = RoomMap[roomNumber][currentAngle];
                    List<TR2Entity> matchingEntities = entities.FindAll(e => e.Room == roomNumber && e.Angle == currentAngle);
                    foreach (TR2Entity match in matchingEntities)
                    {
                        match.X += relocation.X;
                        match.Y += relocation.Y;
                        match.Z += relocation.Z;
                        match.Angle = relocation.Angle;
                    }
                }
            }
        }
    }
}