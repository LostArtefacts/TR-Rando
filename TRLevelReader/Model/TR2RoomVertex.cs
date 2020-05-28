using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2RoomVertex
    {
        public TRVertex Vertex { get; set; }

        public short Lighting { get; set; }

        public ushort Attributes { get; set; }

        public short Lighting2 { get; set; }
    }
}
