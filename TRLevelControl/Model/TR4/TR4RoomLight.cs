﻿using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4RoomLight : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public TRColour Colour { get; set; }

    public byte LightType { get; set; }

    public byte Unknown { get; set; }

    public byte Intensity { get; set; }

    public float In { get; set; }

    public float Out { get; set; }

    public float Length { get; set; }

    public float CutOff { get; set; }

    public float Dx { get; set; }

    public float Dy { get; set; }

    public float Dz { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (TRLevelWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Colour);
            writer.Write(LightType);
            writer.Write(Unknown);
            writer.Write(Intensity);
            writer.Write(In);
            writer.Write(Out);
            writer.Write(Length);
            writer.Write(CutOff);
            writer.Write(Dx);
            writer.Write(Dy);
            writer.Write(Dz);
        }

        return stream.ToArray();
    }
}
