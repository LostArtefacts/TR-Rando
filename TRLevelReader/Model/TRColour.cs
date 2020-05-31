using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRColour
    {
        public byte Red { get; set; }

        public byte Green { get; set; }
        
        public byte Blue { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Red: " + Red);
            sb.Append(" Green: " + Green);
            sb.Append(" Blue: " + Blue);

            return sb.ToString();
        }
    }
}
