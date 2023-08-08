using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes;

public class FDPortalEntry : FDEntry
{
    public ushort Room { get; set; }

    public override ushort[] Flatten()
    {
        return new ushort[]
        {
            Setup.Value,
            Room
        };
    }
}
