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
    }
}
