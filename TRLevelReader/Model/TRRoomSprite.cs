using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    // 4 bytes
    public class TRRoomSprite
    {
        public short Vertex { get; set; }

        public short Texture { get; set; }
    }
}
