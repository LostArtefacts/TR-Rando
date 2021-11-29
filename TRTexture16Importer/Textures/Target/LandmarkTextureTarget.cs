using System.Collections.Generic;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures
{
    public class LandmarkTextureTarget
    {
        public int MappedTextureIndex { get; set; }
        public int BackgroundIndex { get; set; }
        public BitmapGraphics Background { get; set; }
        public int RoomNumber { get; set; }
        public List<int> RectangleIndices { get; set; }

        public LandmarkTextureTarget()
        {
            MappedTextureIndex = -1;
            BackgroundIndex = -1;
            RectangleIndices = new List<int>();
        }
    }
}