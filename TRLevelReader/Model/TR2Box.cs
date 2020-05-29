using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2Box
    {
        public byte ZMin { get; set; }

        public byte ZMax { get; set; }

        public byte XMin { get; set; }

        public byte XMax { get; set; }

        public short TrueFloor { get; set; }

        public short OverlapIndex { get; set; }
    }
}
