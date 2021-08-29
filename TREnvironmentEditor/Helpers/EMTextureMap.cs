using System.Collections.Generic;

namespace TREnvironmentEditor.Helpers
{
    // Texture index => room index => rect/tri indices
    public class EMTextureMap : Dictionary<ushort, EMGeometryMap> { }

    // Room index => rect/tri indices
    public class EMGeometryMap : Dictionary<int, Dictionary<EMTextureFaceType, int[]>> { }

    public enum EMTextureFaceType
    {
        Rectangles,
        Triangles
    }
}