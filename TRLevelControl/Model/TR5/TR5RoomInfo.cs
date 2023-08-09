using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5RoomInfo : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int yBottom { get; set; }

    public int yTop { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Z);
                writer.Write(yBottom);
                writer.Write(yTop);
            }

            return stream.ToArray();
        }
    }
}
