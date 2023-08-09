using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR2RoomLight : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public ushort Intensity1 { get; set; }

    public ushort Intensity2 { get; set; }

    public uint Fade1 { get; set; }

    public uint Fade2 { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);
        sb.Append(" Int1: " + Intensity1);
        sb.Append(" Int2: " + Intensity2);
        sb.Append(" Fade1: " + Fade1);
        sb.Append(" Fade2: " + Fade2);

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
            writer.Write(Intensity1);
            writer.Write(Intensity2);
            writer.Write(Fade1);
            writer.Write(Fade2);
        }

        return stream.ToArray();
    }
}
