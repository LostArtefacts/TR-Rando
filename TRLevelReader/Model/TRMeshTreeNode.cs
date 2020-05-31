using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRMeshTreeNode
    {
        public uint Flags { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public int OffsetZ { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Flags: " + Flags.ToString("X8"));
            sb.Append(" OffsetX: " + OffsetX);
            sb.Append(" OffsetY: " + OffsetY);
            sb.Append(" OffsetZ: " + OffsetZ);

            return sb.ToString();
        }
    }
}
