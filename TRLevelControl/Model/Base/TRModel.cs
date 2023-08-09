using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRModel : ISerializableCompact
{
    public uint ID { get; set; }

    public ushort NumMeshes { get; set; }

    public ushort StartingMesh { get; set; }

    public uint MeshTree { get; set; }

    public uint FrameOffset { get; set; }

    public ushort Animation { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" ID: " + ID);
        sb.Append(" NumMeshes: " + NumMeshes);
        sb.Append(" StartingMesh: " + StartingMesh);
        sb.Append(" MeshTree: " + MeshTree);
        sb.Append(" FrameOffset: " + FrameOffset);
        sb.Append(" Animation: " + Animation);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(ID);
            writer.Write(NumMeshes);
            writer.Write(StartingMesh);
            writer.Write(MeshTree);
            writer.Write(FrameOffset);
            writer.Write(Animation);
        }

        return stream.ToArray();
    }
}
