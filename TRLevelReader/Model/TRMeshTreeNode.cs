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
    }
}
