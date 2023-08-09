using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRRoomVertex : ISerializableCompact
{
    public TRVertex Vertex { get; set; }

    public short Lighting { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Vertex.Serialize());
                writer.Write(Lighting);
            }

            return stream.ToArray();
        }
    }
}
