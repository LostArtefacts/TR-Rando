using System.Collections.Generic;
using System.Drawing;

namespace TRTexture16Importer.Textures.Target
{
    public class DynamicTextureTarget
    {
        public Dictionary<int, List<Rectangle>> DefaultTileTargets { get; set; }
        public Dictionary<int, List<Rectangle>> OptionalTileTargets { get; set; }

        public DynamicTextureTarget()
        {
            DefaultTileTargets = new Dictionary<int, List<Rectangle>>();
            OptionalTileTargets = new Dictionary<int, List<Rectangle>>();
        }
    }
}