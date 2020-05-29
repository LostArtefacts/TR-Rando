using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRObjectTexture
    {
        public ushort Attribute { get; set; }

        public ushort AtlasAndFlag { get; set; }

        public TRObjectTextureVert[] Vertices { get; set; }
    }
}
