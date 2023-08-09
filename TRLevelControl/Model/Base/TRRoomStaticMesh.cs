using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRRoomStaticMesh : ISerializableCompact
{
    public uint X { get; set; }

    public uint Y { get; set; }

    public uint Z { get; set; }

    public ushort Rotation { get; set; }

    public ushort Intensity { get; set; }

    public ushort MeshID { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Rotation);
            writer.Write(Intensity);
            writer.Write(MeshID);
        }

        return stream.ToArray();
    }
}
