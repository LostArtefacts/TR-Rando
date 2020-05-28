using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRMesh
    {
        public TRVertex Centre { get; set; }

        public int CollRadius { get; set; }

        public short NumVetices { get; set; }

        public TRVertex[] Vertices { get; set; }

        public short NumNormals { get; set; }

        public TRVertex[] Normals { get; set; }

        public short[] Lights { get; set; }

        public short NumTexturedRectangles { get; set; }

        public TRFace4[] TexturedRectangles { get; set; }

        public short NumTexturedTriangles { get; set; }

        public TRFace3[] TexturedTriangles { get; set; }

        public short NumColouredRectangles { get; set; }

        public TRFace4[] ColouredRectangles { get; set; }

        public short NumColouredTriangles { get; set; }

        public TRFace3[] ColouredTriangles { get; set; }
    }
}
