using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRMesh
    {
        //6 Bytes
        public TRVertex Centre { get; set; }

        //4 Bytes
        public int CollRadius { get; set; }

        //2 Bytes
        public short NumVetices { get; set; }

        //NumVertices * 6 Bytes
        public TRVertex[] Vertices { get; set; }

        //2 bytes - if this is negative, lights is populated. Otherwise Normals populated
        public short NumNormals { get; set; }

        //NumNormals * 6 bytes OR
        public TRVertex[] Normals { get; set; }

        //NumNormals * 2 bytes - It's either Normals (+) or Lights (-) not both
        public short[] Lights { get; set; }

        //2 bytes
        public short NumTexturedRectangles { get; set; }

        //NumTexturedRectangles * 10 bytes
        public TRFace4[] TexturedRectangles { get; set; }

        //2 bytes
        public short NumTexturedTriangles { get; set; }

        //NumTexturedTriangles * 8 bytes
        public TRFace3[] TexturedTriangles { get; set; }

        //2 bytes
        public short NumColouredRectangles { get; set; }

        //NumColouredRectangles * 10 bytes
        public TRFace4[] ColouredRectangles { get; set; }

        //2 bytes
        public short NumColouredTriangles { get; set; }

        //NumColouredTriangles * 8 bytes
        public TRFace3[] ColouredTriangles { get; set; }

        public TRMesh()
        {
            Centre = new TRVertex { X = 0, Y = 0, Z = 0 };
        }
    }
}
