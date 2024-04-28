using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRSpriteSequence : ISerializableCompact
{
    public int SpriteID { get; set; }

    public short NegativeLength { get; set; }

    public short Offset { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" SpriteID: " + SpriteID);
        sb.Append(" NegativeLength: " + NegativeLength);
        sb.Append(" Offset: " + Offset);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(SpriteID);
            writer.Write(NegativeLength);
            writer.Write(Offset);
        }

        return stream.ToArray();
    }
}
