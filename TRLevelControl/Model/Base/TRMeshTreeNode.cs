using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRMeshTreeNode : ISerializableCompact
{
    public uint Flags { get; set; }

    public int OffsetX { get; set; }

    public int OffsetY { get; set; }

    public int OffsetZ { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Flags: " + Flags.ToString("X8"));
        sb.Append(" OffsetX: " + OffsetX);
        sb.Append(" OffsetY: " + OffsetY);
        sb.Append(" OffsetZ: " + OffsetZ);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Flags);
            writer.Write(OffsetX);
            writer.Write(OffsetY);
            writer.Write(OffsetZ);
        }

        return stream.ToArray();
    }
}
