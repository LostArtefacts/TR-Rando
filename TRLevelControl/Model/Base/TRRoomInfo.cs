using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRRoomInfo : ISerializableCompact
{
    public int X { get; set; }

    public int Z { get; set; }

    public int YBottom { get; set; }

    public int YTop { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Z: " + Z);
        sb.Append(" YBottom: " + YBottom);
        sb.Append(" YTop: " + YTop);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Z);
            writer.Write(YBottom);
            writer.Write(YTop);
        }

        return stream.ToArray();
    }
}
