﻿using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

//10 bytes - rosetta stone says 12, i think thats a mistype/miscalc
public class TRFace4 : ISerializableCompact
{
    //4 vertices in a quad
    public ushort[] Vertices { get; set; }

    public ushort Texture { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            foreach (ushort vert in Vertices)
            {
                writer.Write(vert);
            }

            writer.Write(Texture);
        }

        return stream.ToArray();
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        int index = 0;
        foreach (ushort vertex in Vertices)
        {
            sb.Append(" Vertex[" + index + "]: " + vertex);
            index++;
        }

        sb.Append(" Texture: " + Texture);

        return sb.ToString();
    }
}
