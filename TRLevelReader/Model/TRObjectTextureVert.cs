using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRObjectTextureVert
    {
        //Both are ufixed16 - 1 byte whole 1 byte fractional
        public FixedFloat<byte, byte> XCoordinate { get; set; }

        public FixedFloat<byte, byte> YCoordinate { get; set; }
    }
}
