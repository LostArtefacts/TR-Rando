using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR5RoomInfo : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int YBottom { get; set; }

    public int YTop { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(YBottom);
            writer.Write(YTop);
        }

        return stream.ToArray();
    }
}
