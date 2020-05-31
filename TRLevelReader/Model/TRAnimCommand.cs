using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRAnimCommand
    {
        public short Value { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Value: " + Value.ToString("0x{0:X4}"));

            return sb.ToString();
        }
    }
}
