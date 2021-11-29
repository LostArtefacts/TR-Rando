using System.Drawing;

namespace TRTexture16Importer.Textures
{
    public class StaticTextureTarget
    {
        private static readonly Rectangle _noClip = new Rectangle(-1, -1, 0, 0);

        public int Segment { get; set; }
        public int Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// If Clip == _noClip (default), the Source rectangle is used. Otherwise the
        /// source rectangle is clipped according to this value.
        /// </summary>
        public Rectangle Clip { get; set; }

        public bool ClipRequired => Clip != _noClip;

        public bool Clear { get; set; }

        public StaticTextureTarget()
        {
            Clip = _noClip;
            Clear = false;
        }
    }
}