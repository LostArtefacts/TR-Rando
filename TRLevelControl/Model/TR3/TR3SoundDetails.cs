﻿using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3SoundDetails : ISerializableCompact
{
    public ushort Sample { get; set; }

    public byte Volume { get; set; }

    public byte Range { get; set; }

    public byte Chance { get; set; }

    public byte Pitch { get; set; }

    public short Characteristics { get; set; }

    public int NumSounds => (Characteristics & 0x00FC) >> 2; // get bits 2-7

    public byte LoopingMode
    {
        get
        {
            return (byte)(Characteristics & 3);
        }
        set
        {
            Characteristics = (short)(Characteristics & ~(Characteristics & 3));
            Characteristics |= (short)(value & 3);
        }
    }

    public bool Wibble
    {
        get
        {
            return (Characteristics & 0x2000) > 0;
        }
        set
        {
            if (value)
            {
                Characteristics |= 0x2000;
            }
            else
            {
                Characteristics = (short)(Characteristics & ~0x2000);
            }
        }
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Sample);
            writer.Write(Volume);
            writer.Write(Range);
            writer.Write(Chance);
            writer.Write(Pitch);
            writer.Write(Characteristics);
        }

        return stream.ToArray();
    }
}
