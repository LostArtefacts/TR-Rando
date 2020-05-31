using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    //8 bytes
    public class TRFace3
    {
        // 3 vertices in a triangle
        public ushort[] Vertices { get; set; }

        public ushort Texture { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            int index = 0;
            foreach (ushort vertex in Vertices)
            {
                sb.Append(" Vertex[" + index + "]: " + vertex);
                index++;
            }

            sb.Append(" Texture: " + Texture);

            return sb.ToString();
        }
    }
}
