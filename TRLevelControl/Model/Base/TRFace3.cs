using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

//8 bytes
public class TRFace3 : ISerializableCompact
{
    // 3 vertices in a triangle
    public ushort[] Vertices { get; set; }

    public ushort Texture { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new())
        {
            using (BinaryWriter writer = new(stream))
            {
                foreach (ushort vertex in Vertices)
                {
                    writer.Write(vertex);
                }

                writer.Write(Texture);
            }

            return stream.ToArray();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        int index = 0;
        foreach (ushort vertex in Vertices)
        {
            sb.Append(" Vertex[" + index + "]: " + vertex);
            index++;
        }

        sb.Append(" Texture: " + Texture);

        return sb.ToString();
    }
}
