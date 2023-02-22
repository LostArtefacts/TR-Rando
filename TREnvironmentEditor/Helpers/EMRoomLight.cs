using TRLevelReader.Model;

namespace TREnvironmentEditor.Helpers
{
    public class EMRoomLight
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public ushort Intensity1 { get; set; }
        public ushort Intensity2 { get; set; }
        public uint Fade1 { get; set; }
        public uint Fade2 { get; set; }
        public TRColour Colour { get; set; }
        public byte LightType { get; set; }
        public short[] LightProperties { get; set; }
    }
}
