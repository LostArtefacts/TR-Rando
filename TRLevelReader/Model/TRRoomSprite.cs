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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Vertex: " + Vertex);
            sb.Append("Texture: " + Texture);

            return sb.ToString();
        }
    }
}
