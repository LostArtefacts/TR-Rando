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
        }

        public FDTrigAction TrigAction
        {
            get
            {
                return (FDTrigAction)(Value & 0x7C00);
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
