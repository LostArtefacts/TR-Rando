using TRLevelReader.Model.Enums;

namespace TRModelTransporter.Model
{
    public class PackedAnimationCommand
    {
        public TR2AnimCommand Command { get; set; }
        public short[] Params { get; set; }
    }
}