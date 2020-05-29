using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRRoomData
    {
        // 2 bytes
        public short NumVertices { get; set; }

        // NumVertices * 12 bytes
        public TR2RoomVertex[] Vertices { get; set; }

        // 2 bytes
        public short NumRectangles { get; set; }

        // NumRectangles * 10 bytes
        public TRFace4[] Rectangles { get; set; }

        // 2 bytes
        public short NumTriangles { get; set; }

        // NumTriangles * 8 bytes
        public TRFace3[] Triangles { get; set; }

        // 2 bytes
        public short NumSprites { get; set; }

        // NumSprites * 4 bytes bytes
        public TRRoomSprite[] Sprites { get; set; }
    }
}
