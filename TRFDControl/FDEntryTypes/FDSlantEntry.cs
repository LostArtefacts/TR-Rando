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

        public sbyte XSlant
        {
            get
            {
                return (sbyte)(SlantValue & 0x00ff);
            }
            set
            {
                SlantValue = (ushort)(SlantValue & ~XSlant);
                SlantValue = (ushort)(SlantValue | (int)value);
            }
        }

        public sbyte ZSlant
        {
            get
            {
                return (sbyte)(SlantValue >> 8);
            }
            set
            {
                SlantValue = (ushort)(SlantValue & ~ZSlant);
                SlantValue = (ushort)(SlantValue | value << 8);
            }
        }

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
