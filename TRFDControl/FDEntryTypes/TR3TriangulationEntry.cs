using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes
{
    public class TR3TriangulationEntry : FDEntry
    {
        public FDTriangulationData TriData { get; set; }

        public override ushort[] Flatten()
        {
            return new ushort[]
            {
                Setup.Value,
                TriData.Value
            };
        }
    }
}
