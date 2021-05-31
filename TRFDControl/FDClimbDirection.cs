using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public enum FDClimbDirection
    {
        PositiveZ = 0x01,
        PositiveX = 0x02,
        NegativeZ = 0x04,
        NegativeX = 0x08
    }
}
