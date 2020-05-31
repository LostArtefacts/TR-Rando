using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRSoundDetails
    {
        public ushort Sample { get; set; }

        public ushort Volume { get; set; }

        public ushort Chance { get; set; }

        public ushort Characteristics { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Sample: " + Sample);
            sb.Append(" Volume: " + Volume);
            sb.Append(" Chance: " + Chance);
            sb.Append(" Characteristics: " + Characteristics.ToString("0x{0:X4}"));

            return sb.ToString();
        }
    }
}
