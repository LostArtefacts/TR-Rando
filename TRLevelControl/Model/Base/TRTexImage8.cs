using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRTexImage8 : ISerializableCompact
{
    public byte[] Pixels { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new())
        {
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(Pixels);
            }

            return stream.ToArray();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append("\n");

        int Count = 1;
        foreach (byte pixel in Pixels)
        {
            sb.Append(pixel + " ");

            Count++;

            if (Count % 8 == 0)
            {
                sb.Append("\n");
            }
        }

        return sb.ToString();
    }
}
