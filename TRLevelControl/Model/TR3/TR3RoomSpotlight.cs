﻿using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomSpotlight : ISerializableCompact
{
    public int Intensity { get; set; }

    public int Fade { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Intensity);
            writer.Write(Fade);
        }

        return stream.ToArray();
    }
}
