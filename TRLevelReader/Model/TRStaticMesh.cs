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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" ID: " + ID);
            sb.Append(" Mesh: " + Mesh);
            sb.Append(" VisibilityBox: {" + VisibilityBox.ToString() + "} ");
            sb.Append(" CollisionBox: {" + CollisionBox.ToString() + "} ");
            sb.Append(" Flags: " + Flags.ToString("0x{0:X4}"));

            return sb.ToString();
        }
    }
}
