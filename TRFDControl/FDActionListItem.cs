using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public class FDActionListItem
    {
        public ushort Value { get; set; }

        public ushort Parameter
        {
            get
            {
                return (ushort)(Value & 0x03FF);
            }
            set
            {
                // Remove the current parameter, and add the new one
                Value = (ushort)((Value & ~Parameter) | value);
            }
        }

        public FDTrigAction TrigAction
        {
            get
            {
                return (FDTrigAction)((Value & 0x7C00) >> 10); // See Control.c line 30
            }
            set
            {
                Value = (ushort)(Value & ~(Value & 0x7C00));
                Value |= (ushort)((byte)value << 10);
            }
        }

        public bool Continue
        {
            get
            {
                //Continue bit set to 0 means to continue, not 1...
                return !((Value & 0x8000) > 0);
            }
            internal set
            {
                if (value)
                {
                    Value = (ushort)(Value & ~0x8000);
                }
                else
                {
                    Value |= 0x8000;
                }
            }
        }

        public FDCameraAction CamAction { get; set; }
    }
}
