using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRRoomPortal
    {
        public ushort AdjoiningRoom { get; set; }

        public TRVertex Normal { get; set; }

        public TRVertex[] Vertices { get; set; }
    }
}
