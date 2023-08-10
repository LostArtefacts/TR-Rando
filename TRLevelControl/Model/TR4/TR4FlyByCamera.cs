using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4FlyByCamera : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public int Dx { get; set; }

    public int Dy { get; set; }

    public int Dz { get; set; }

    public byte Sequence { get; set; }

    public byte Index { get; set; }

    public ushort FOV { get; set; }

    public short Roll { get; set; }

    public ushort Timer { get; set; }

    public ushort Speed { get; set; }

    public ushort Flags { get; set; }

    public uint RoomID { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Dx);
            writer.Write(Dy);
            writer.Write(Dz);
            writer.Write(Sequence);
            writer.Write(Index);
            writer.Write(FOV);
            writer.Write(Roll);
            writer.Write(Timer);
            writer.Write(Speed);
            writer.Write(Flags);
            writer.Write(RoomID);
        }

        return stream.ToArray();
    }
}
