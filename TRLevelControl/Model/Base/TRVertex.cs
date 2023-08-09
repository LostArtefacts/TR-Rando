using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

//6 bytes
public class TRVertex : ISerializableCompact
{
    public short X { get; set; }

    public short Y { get; set; }

    public short Z { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Z);
            }

            return stream.ToArray();
        }
    }
}
