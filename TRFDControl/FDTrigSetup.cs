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
            set
            {
                Value = (ushort)(Value & ~(Value & 0x00FF));
                Value |= value;
            }
        }

        public bool OneShot
        {
            get
            {
                return (Value & 0x0100) > 0;
            }
            set
            {
                if (value)
                {
                    Value |= 0x0100;
                }
                else
                {
                    // Rather than Xor'ing, this allows successive calls with the same bool val
                    Value = (ushort)(Value & ~0x0100);
                }
            }
        }

        public byte Mask
        {
            get
            {
                return (byte)((Value & 0x3E00) >> 9);
            }
            set
            {
                Value = (ushort)(Value & ~(Value & 0x3E00));
                Value |= (ushort)(value << 9);
            }
        }
    }
}
