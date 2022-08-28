using System;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMModifyRoomFunction : BaseEMFunction
    {
        public int[] Rooms { get; set; }
        public bool? IsSkyboxVisible { get; set; }
        public bool? IsWindy { get; set; }
        public bool? IsSwamp { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            throw new NotSupportedException();
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            foreach (int roomNumber in Rooms)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
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
            EMLevelData data = GetData(level);
            foreach (int roomNumber in Rooms)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                if (IsSkyboxVisible.HasValue)
                {
                    room.IsSkyboxVisible = IsSkyboxVisible.Value;
                }
                if (IsWindy.HasValue)
                {
                    room.IsWindy = IsWindy.Value;
                }
                if (IsSwamp.HasValue)
                {
                    room.IsSwamp = IsSwamp.Value;
                }
            }
        }
    }
}