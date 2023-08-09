using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5RoomLight : ISerializableCompact
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }

    public float R { get; set; }

    public float G { get; set; }

    public float B { get; set; }

    public uint Seperator { get; set; }

    public float In { get; set; }

    public float Out { get; set; }

    public float RadIn { get; set; }

    public float RadOut { get; set; }

    public float Range { get; set; }

    public float DX { get; set; }

    public float DY { get; set; }

    public float DZ { get; set; }

    public int X2 { get; set; }

    public int Y2 { get; set; }

    public int Z2 { get; set; }

    public int DX2 { get; set; }

    public int DY2 { get; set; }

    public int DZ2 { get; set; }

    public byte LightType { get; set; }

    public byte[] Filler { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(R);
            writer.Write(G);
            writer.Write(B);
            writer.Write(Seperator);
            writer.Write(In);
            writer.Write(Out);
            writer.Write(RadIn);
            writer.Write(RadOut);
            writer.Write(Range);
            writer.Write(DX);
            writer.Write(DY);
            writer.Write(DZ);
            writer.Write(X2);
            writer.Write(Y2);
            writer.Write(Z2);
            writer.Write(DX2);
            writer.Write(DY2);
            writer.Write(DZ2);
            writer.Write(LightType);
            writer.Write(Filler);
        }

        return stream.ToArray();
    }
}
