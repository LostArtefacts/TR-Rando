using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    //32 bytes
    public class TRRoomPortal
    {
        public ushort AdjoiningRoom { get; set; }

        public TRVertex Normal { get; set; }

        // 4 vertices
        public TRVertex[] Vertices { get; set; }
    }
}
