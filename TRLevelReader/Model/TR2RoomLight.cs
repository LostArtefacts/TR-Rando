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
    }
}
