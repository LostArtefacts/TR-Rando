using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRModel
    {
        public uint ID { get; set; }

        public ushort NumMeshes { get; set; }

        public ushort StartingMesh { get; set; }

        public uint MeshTree { get; set; }

        public uint FrameOffset { get; set; }

        public ushort Animation { get; set; }
    }
}
