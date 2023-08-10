using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class FixedFloat<T, U>
{
    public T Whole { get; set; }

    public U Fraction { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Whole: " + Whole);
        sb.Append(" Fraction: " + Fraction);

        return sb.ToString();
    }
}

public sealed class FixedFloat32 : FixedFloat<short, ushort>, ISerializableCompact
{
    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Whole);
            writer.Write(Fraction);
        }

        return stream.ToArray();
    }
}

public sealed class FixedFloat16 : FixedFloat<byte, byte>, ISerializableCompact
{
    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Whole);
            writer.Write(Fraction);
        }

        return stream.ToArray();
    }
}
