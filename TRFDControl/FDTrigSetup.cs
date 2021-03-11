using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public class FDTrigSetup
    {
        public ushort Value { get; set; }

        public byte Timer
        {
            get
            {
                return (byte)(Value & 0x00FF);
            }
        }

        public bool OneShot
        {
            get
            {
                return (Value & 0x0100) > 0;
            }
        }

        public byte Mask
        {
            get
            {
                return (byte)(Value & 0x3E00);
            }
        }
    }
}
