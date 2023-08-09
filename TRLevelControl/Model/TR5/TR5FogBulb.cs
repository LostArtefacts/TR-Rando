using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5FogBulb : ISerializableCompact
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

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
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
            }

            return stream.ToArray();
        }
    }
}
