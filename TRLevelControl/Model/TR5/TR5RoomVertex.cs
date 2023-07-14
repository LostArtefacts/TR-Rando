using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR5RoomVertex : ISerializableCompact
    {
        public TR5Vertex Vert { get; set; }

        public TR5Vertex Norm { get; set; }

        public uint Colour { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Vert.Serialize());
                    writer.Write(Norm.Serialize());
                    writer.Write(Colour);
                }

                return stream.ToArray();
            }
        }
    }
}
