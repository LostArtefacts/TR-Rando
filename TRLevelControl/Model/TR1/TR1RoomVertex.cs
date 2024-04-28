using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR1RoomVertex : ISerializableCompact
{
    public TRVertex Vertex { get; set; }

    public short Lighting { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Vertex.Serialize());
            writer.Write(Lighting);
        }

        return stream.ToArray();
    }
}
