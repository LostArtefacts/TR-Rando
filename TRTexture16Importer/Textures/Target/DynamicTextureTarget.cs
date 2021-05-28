using System.Collections.Generic;
using System.Drawing;

namespace TRTexture16Importer.Textures.Target
{
    public class DynamicTextureTarget
    {
        public Dictionary<int, List<Rectangle>> TileTargets { get; set; }

        public DynamicTextureTarget()
        {
            TileTargets = new Dictionary<int, List<Rectangle>>();
        }
    }
}