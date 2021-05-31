using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public class FDCameraAction
    {
        public ushort Value { get; set; }

        public byte Timer
        {
            get
            {
                return (byte)(Value & 0x00FF);
            }
        }

        public bool Once
        {
            get
            {
                return (Value & 0x0100) > 0;
            }
        }

        public byte MoveTimer
        {
            get
            {
                return (byte)(Value & 0x3E00);
            }
        }

        public bool Continue
        {
            get
            {
                //Continue bit set to 0 means to continue, not 1...
                return !((Value & 0x8000) > 0);
            }
        }
    }
}
