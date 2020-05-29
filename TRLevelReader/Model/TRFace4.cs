using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    //10 bytes - rosetta stone says 12, i think thats a mistype/miscalc
    public class TRFace4
    {
        //4 vertices in a quad
        public ushort[] Vertices { get; set; }

        public ushort Texture { get; set; }
    }
}
