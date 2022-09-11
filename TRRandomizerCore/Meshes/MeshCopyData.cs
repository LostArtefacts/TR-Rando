using System.Collections.Generic;
using TRLevelReader.Model;

namespace TRRandomizerCore.Meshes
{
    public class MeshCopyData
    {
        public TRMesh BaseMesh { get; set; }
        public TRMesh NewMesh { get; set; }
        public IEnumerable<int> TextureFaceCopies { get; set; }
        public IEnumerable<int> ColourFaceCopies { get; set; }
    }
}