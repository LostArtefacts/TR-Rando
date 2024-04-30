using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR3RoomVertex : TRRoomVertex, ISerializableCompact
{
    public short Lighting { get; set; }
    public ushort Attributes { get; set; }
    public ushort Colour { get; set; }

    public bool UseWaveMovement
    {
        get => (Attributes & 0x2000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x2000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x2000);
            }
        }
    }

    public bool UseCaustics
    {
        get => (Attributes & 0x4000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x4000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x4000);
            }
        }
    }

    // Temp for TR4
    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Vertex.Serialize());
            writer.Write(Lighting);
            writer.Write(Attributes);
            writer.Write(Colour);
        }

        return stream.ToArray();
    }
}
