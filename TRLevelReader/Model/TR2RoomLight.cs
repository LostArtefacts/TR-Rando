using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2RoomLight
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public ushort Intensity1 { get; set; }

        public ushort Intensity2 { get; set; }

        public uint Fade1 { get; set; }

        public uint Fade2 { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Z: " + Z);
            sb.Append(" Int1: " + Intensity1);
            sb.Append(" Int2: " + Intensity2);
            sb.Append(" Fade1: " + Fade1);
            sb.Append(" Fade2: " + Fade2);

            return sb.ToString();
        }
    }
}
