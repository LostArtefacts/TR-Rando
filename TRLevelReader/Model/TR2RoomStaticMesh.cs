using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR2RoomStaticMesh
    {
        public uint X { get; set; }

        public uint Y { get; set; }

        public uint Z { get; set; }

        public ushort Rotation { get; set; }

        public ushort Intensity1 { get; set; }

        public ushort Intensity2 { get; set; }

        public ushort MeshID { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Z: " + Z);
            sb.Append(" Rotation: " + Rotation);
            sb.Append(" Int1: " + Intensity1);
            sb.Append(" Int2: " + Intensity2);
            sb.Append(" MeshID: " + MeshID);

            return sb.ToString();
        }
    }
}
