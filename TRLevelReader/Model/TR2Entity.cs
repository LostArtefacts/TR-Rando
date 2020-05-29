using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2Entity
    {
        public short TypeID { get; set; }

        public short Room { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public short Angle { get; set; }

        public short Intensity1 { get; set; }

        public short Intensity2 { get; set; }

        public ushort Flags { get; set; }
    }
}
