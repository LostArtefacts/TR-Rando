using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes
{
    public class FDClimbEntry : FDEntry
    {
        public bool IsPositiveX
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.PositiveX) > 0);
            }
        }

        public bool IsPositiveZ
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.PositiveZ) > 0);
            }
        }

        public bool IsNegativeX
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.NegativeX) > 0);
            }
        }

        public bool IsNegativeZ
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.NegativeZ) > 0);
            }
        }
    }
}
