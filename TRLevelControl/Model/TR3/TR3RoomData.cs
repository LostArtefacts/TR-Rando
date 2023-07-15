using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR3RoomData : ISerializableCompact
    {
        // 2 bytes
        public short NumVertices { get; set; }

        // NumVertices * 12 bytes
        public TR3RoomVertex[] Vertices { get; set; }

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

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(NumVertices);

                    foreach (TR3RoomVertex vert in Vertices)
                    {
                        writer.Write(vert.Serialize());
                    }

                    writer.Write(NumRectangles);

                    foreach (TRFace4 face in Rectangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    writer.Write(NumTriangles);

                    foreach (TRFace3 face in Triangles)
                    {
                        writer.Write(face.Serialize());
                    }

                    writer.Write(NumSprites);

                    foreach (TRRoomSprite sprite in Sprites)
                    {
                        writer.Write(sprite.Serialize());
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
