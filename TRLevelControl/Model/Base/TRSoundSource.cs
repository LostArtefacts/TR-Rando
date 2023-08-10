using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRSoundSource : ISerializableCompact
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Z { get; set; }

    public ushort SoundID { get; set; }

    public ushort Flags { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" X: " + X);
        sb.Append(" Y: " + Y);
        sb.Append(" Z: " + Z);
        sb.Append(" SoundID: " + SoundID);
        sb.Append(" Flags: " + Flags.ToString("X4"));

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
            writer.Write(SoundID);
            writer.Write(Flags);
        }

        return stream.ToArray();
    }
}
