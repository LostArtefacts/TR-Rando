using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRRoomData
    {
        public short NumVertices { get; set; }

        public TR2RoomVertex[] Vertices { get; set; }

        public short NumRectangles { get; set; }

        public TRFace4[] Rectangles { get; set; }

        public short NumTriangles { get; set; }

        public TRFace3[] Triangles { get; set; }

        public short NumSprites { get; set; }

        public TRRoomSprite[] Sprites { get; set; }
    }
}
