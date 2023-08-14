using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRColour : ISerializableCompact
{
    public byte Red { get; set; }
    public byte Green { get; set; }    
    public byte Blue { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} R: {Red} G: {Green} B: {Blue}";
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Red);
            writer.Write(Green);
            writer.Write(Blue);
        }

        return stream.ToArray();
    }
}
