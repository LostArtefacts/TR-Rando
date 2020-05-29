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
    }
}
