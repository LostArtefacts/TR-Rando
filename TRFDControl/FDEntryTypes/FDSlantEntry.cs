using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes
{
    public class FDSlantEntry : FDEntry
    {
        public FDSlantEntryType Type { get; set; }

        public ushort SlantValue { get; set; }

        public override ushort[] Flatten()
        {
            return new ushort[]
            {
                Setup.Value,
                SlantValue
            };
        }
    }
}
