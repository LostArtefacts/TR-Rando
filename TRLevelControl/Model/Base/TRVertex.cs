using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRVertex : ISerializableCompact, ICloneable
{
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public TRVertex Clone()
    {
        return new()
        {
            X = X,
            Y = Y,
            Z = Z
        };
    }

    object ICloneable.Clone()
        => Clone();

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);

        return sb.ToString();
    }

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
