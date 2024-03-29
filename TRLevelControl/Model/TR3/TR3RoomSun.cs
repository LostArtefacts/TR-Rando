﻿using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomSun : ISerializableCompact
{
    public short NormalX { get; set; }

    public short NormalY { get; set; }

    public short NormalZ { get; set; }

    public short Unused { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(NormalX);
            writer.Write(NormalY);
            writer.Write(NormalZ);
            writer.Write(Unused);
        }

        return stream.ToArray();
    }
}
