using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class FixedFloat
    {
        public short Whole { get; set; }

        public ushort Fraction { get; set; }
    }

    public class FixedShortFloat
    {
        public byte Whole { get; set; }

        public byte Fraction { get; set; }
    }
}
