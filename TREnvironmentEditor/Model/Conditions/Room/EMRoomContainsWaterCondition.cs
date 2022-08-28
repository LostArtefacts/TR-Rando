using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMRoomContainsWaterCondition : BaseEMCondition
    {
        public short RoomIndex { get; set; }

        protected override bool Evaluate(TRLevel level)
        {
            TRRoom room = level.Rooms[RoomIndex];
            return room.ContainsWater;
        }

        protected override bool Evaluate(TR2Level level)
        {
            TR2Room room = level.Rooms[RoomIndex];
            return room.ContainsWater;
        }

        protected override bool Evaluate(TR3Level level)
        {
            TR3Room room = level.Rooms[RoomIndex];
            return room.ContainsWater;
        }
    }
}