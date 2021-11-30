using System;
using System.Collections.Generic;
using System.Drawing;

namespace TRTexture16Importer.Textures
{
    public class ReplacementTextureTarget
    {
        public TilePoint Search { get; set; }
        public TilePoint Replace { get; set; }
        public Dictionary<int, List<Rectangle>> ReplacementMap { get; set; }
    }

    public class TilePoint
    {
        public int Tile { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}