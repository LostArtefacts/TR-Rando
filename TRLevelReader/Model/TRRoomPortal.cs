using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    //32 bytes
    public class TRRoomPortal : ISerializableCompact
    {
        public ushort AdjoiningRoom { get; set; }

        public TRVertex Normal { get; set; }

        // 4 vertices
        public TRVertex[] Vertices { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(AdjoiningRoom);
                    writer.Write(Normal.Serialize());

                    foreach (TRVertex vert in Vertices)
                    {
                        writer.Write(vert.Serialize());
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
