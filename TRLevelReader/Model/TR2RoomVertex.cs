using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2RoomVertex
    {
        //6 bytes
        public TRVertex Vertex { get; set; }

        //2 bytes
        public short Lighting { get; set; }

        //2 bytes
        public ushort Attributes { get; set; }

        //2 bytes
        public short Lighting2 { get; set; }

        public TR2RoomVertex()
        {
            Vertex = new TRVertex();
        }
    }
}
