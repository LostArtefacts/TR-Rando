﻿using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRAnimCommand : ISerializableCompact
{
    public short Value { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Value: " + Value.ToString("X4"));

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Value);
        }

        return stream.ToArray();
    }
}
