using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl;

public class FDEntry
{
    public FDSetup Setup { get; set; }

    public virtual ushort[] Flatten()
    {
        return new ushort[] { Setup.Value };
    }
}
