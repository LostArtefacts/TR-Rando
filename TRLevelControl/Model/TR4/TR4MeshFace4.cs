using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4MeshFace4 : ISerializableCompact
{
    public ushort[] Vertices { get; set; }

    public ushort Texture { get; set; }

    public ushort Effects { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            foreach (ushort vert in Vertices) { writer.Write(vert); }
            writer.Write(Texture);
            writer.Write(Effects);
        }

        return stream.ToArray();
    }
}
