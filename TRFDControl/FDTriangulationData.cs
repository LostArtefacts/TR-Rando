using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public class FDTriangulationData
    {
        public ushort Value { get; set; }

        public byte C10
        {
            get
            {
                return (byte)(Value & 0x000F);
            }
        }

        public byte C00
        {
            get
            {
                return (byte)((Value & 0x00F0) >> 4);
            }
        }

        public byte C01
        {
            get
            {
                return (byte)((Value & 0x0F00) >> 8);
            }
        }

        public byte C11
        {
            get
            {
                return (byte)((Value & 0xF000) >> 12);
            }
        }
    }
}
