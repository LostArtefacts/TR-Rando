using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRSpriteSequence
    {
        public int SpriteID { get; set; }

        public short NegativeLength { get; set; }

        public short Offset { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" SpriteID: " + SpriteID);
            sb.Append(" NegativeLength: " + NegativeLength);
            sb.Append(" Offset: " + Offset);

            return sb.ToString();
        }
    }
}
