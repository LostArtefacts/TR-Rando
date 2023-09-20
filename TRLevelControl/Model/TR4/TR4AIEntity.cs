using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4AIEntity : ISerializableCompact
{
    public short TypeID { get; set; }
    public short Room { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Angle { get; set; }
    public ushort Flags { get; set; }
    public short OCB { get; set; }
    public short Box { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(TypeID);
            writer.Write(Room);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(OCB);
            writer.Write(Flags);
            writer.Write(Angle);
        }

        return stream.ToArray();
    }
}
