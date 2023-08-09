using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRObjectTextureVert : ISerializableCompact
{
    //Both are ufixed16 - 1 byte whole 1 byte fractional
    public FixedFloat16 XCoordinate { get; set; }

    public FixedFloat16 YCoordinate { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(XCoordinate.Serialize());
            writer.Write(YCoordinate.Serialize());
        }

        return stream.ToArray();
    }
}
