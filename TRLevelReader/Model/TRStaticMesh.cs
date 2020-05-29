using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRStaticMesh
    {
        public uint ID { get; set; }

        public ushort Mesh { get; set; }

        public TRBoundingBox VisibilityBox { get; set; }

        public TRBoundingBox CollisionBox { get; set; }

        public ushort Flags { get; set; }
    }
}
