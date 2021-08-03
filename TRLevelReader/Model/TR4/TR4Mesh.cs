using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Mesh : ISerializableCompact
    {
        public TRVertex Centre { get; set; }

        public int CollRadius { get; set; }

        public short NumVertices { get; set; }

        public TRVertex[] Vertices { get; set; }

        public short NumNormals { get; set; }

        public TRVertex[] Normals { get; set; }

        public short[] Lights { get; set; }

        public short NumTexturedRectangles { get; set; }

        public TR4MeshFace4[] TexturedRectangles { get; set; }

        public short NumTexturedTriangles { get; set; }

        public TR4MeshFace3[] TexturedTriangles { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
