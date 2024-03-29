﻿using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR2RoomStaticMesh : ISerializableCompact
{
    public uint X { get; set; }

    public uint Y { get; set; }

    public uint Z { get; set; }

    public ushort Rotation { get; set; }

    public ushort Intensity1 { get; set; }

    public ushort Intensity2 { get; set; }

    public ushort MeshID { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);
        sb.Append(" Rotation: " + Rotation);
        sb.Append(" Int1: " + Intensity1);
        sb.Append(" Int2: " + Intensity2);
        sb.Append(" MeshID: " + MeshID);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Rotation);
            writer.Write(Intensity1);
            writer.Write(Intensity2);
            writer.Write(MeshID);
        }

        return stream.ToArray();
    }
}
