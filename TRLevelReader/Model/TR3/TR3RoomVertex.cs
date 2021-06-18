using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR3RoomVertex : ISerializableCompact
    {
        public TRVertex Vertex { get; set; }

        public short Lighting { get; set; }

        public ushort Attributes { get; set; }

        public ushort Colour { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Vertex.Serialize());
                    writer.Write(Lighting);
                    writer.Write(Attributes);
                    writer.Write(Colour);
                }

                return stream.ToArray();
            }
        }
    }
}
