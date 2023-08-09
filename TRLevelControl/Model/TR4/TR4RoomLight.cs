using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

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

    public float dx { get; set; }

    public float dy { get; set; }

    public float dz { get; set; }

    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Z);
                writer.Write(Colour.Serialize());
                writer.Write(LightType);
                writer.Write(Unknown);
                writer.Write(Intensity);
                writer.Write(In);
                writer.Write(Out);
                writer.Write(Length);
                writer.Write(CutOff);
                writer.Write(dx);
                writer.Write(dy);
                writer.Write(dz);
            }

            return stream.ToArray();
        }
    }
}
