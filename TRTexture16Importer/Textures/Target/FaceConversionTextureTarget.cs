using System.Collections.Generic;

namespace TRTexture16Importer.Textures.Target
{
    public class FaceConversionTextureTarget
    {
        public int RoomNumber { get; set; }
        public Dictionary<int, int> Conversion { get; set; }
        public List<int> RectangleIndices { get; set; }

        public FaceConversionTextureTarget()
        {
            RectangleIndices = new List<int>();
        }
    }
}