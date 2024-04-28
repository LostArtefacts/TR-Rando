using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRBoundingBox : ISerializableCompact, ICloneable
{
    public short MinX { get; set; }

    public short MaxX { get; set; }

    public short MinY { get; set; }

    public short MaxY { get; set; }

    public short MinZ { get; set; }

    public short MaxZ { get; set; }

    public TRBoundingBox Clone()
    {
        return new()
        {
            MinX = MinX,
            MaxX = MaxX,
            MinY = MinY,
            MaxY = MaxY,
            MinZ = MinZ,
            MaxZ = MaxZ
        };
    }

    object ICloneable.Clone()
        => Clone();

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" MinX: " + MinX);
        sb.Append(" MaxX: " + MaxX);
        sb.Append(" MinY: " + MinY);
        sb.Append(" MaxY: " + MaxY);
        sb.Append(" MinZ: " + MinZ);
        sb.Append(" MaxZ: " + MaxZ);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(MinX);
            writer.Write(MaxX);
            writer.Write(MinY);
            writer.Write(MaxY);
            writer.Write(MinZ);
            writer.Write(MaxZ);
        }

        return stream.ToArray();
    }
}
