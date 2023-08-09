using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomSun : ISerializableCompact
{
    public short NormalX { get; set; }

    public short NormalY { get; set; }

    public short NormalZ { get; set; }

    public short Unused { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(NormalX);
                writer.Write(NormalY);
                writer.Write(NormalZ);
                writer.Write(Unused);
            }

            return stream.ToArray();
        }
    }
}
