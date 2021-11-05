using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMModifyRoomFunction : BaseEMFunction
    {
        public int[] Rooms { get; set; }
        public bool? IsSkyboxVisible { get; set; }
        public bool? IsWindy { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            foreach (int roomNumber in Rooms)
            {
                TR2Room room = level.Rooms[roomNumber];
                if (IsSkyboxVisible.HasValue)
                {
                    room.IsSkyboxVisible = IsSkyboxVisible.Value;
                }
                if (IsWindy.HasValue)
                {
                    room.IsWindy = IsWindy.Value;
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            throw new System.NotImplementedException();
        }
    }
}