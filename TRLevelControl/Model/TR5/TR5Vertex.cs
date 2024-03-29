﻿using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5Vertex : ISerializableCompact
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        return stream.ToArray();
    }
}
