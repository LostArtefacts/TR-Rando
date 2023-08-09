using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class FixedFloat<T, U>
{
    public T Whole { get; set; }

    public U Fraction { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());

        sb.Append(" Whole: " + Whole);
        sb.Append(" Fraction: " + Fraction);

        return sb.ToString();
    }
}

public sealed class FixedFloat32 : FixedFloat<short, ushort>, ISerializableCompact
{
    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Whole);
                writer.Write(Fraction);
            }

            return stream.ToArray();
        }
    }
}

public sealed class FixedFloat16 : FixedFloat<byte, byte>, ISerializableCompact
{
    public byte[] Serialize()
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Whole);
                writer.Write(Fraction);
            }

            return stream.ToArray();
        }
    }
}
