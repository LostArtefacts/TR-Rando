using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4ObjectTexture : ISerializableCompact
{
    public ushort Attribute { get; set; }

    public ushort TileAndFlag { get; set; }

    public ushort NewFlags { get; set; }

    public TRObjectTextureVert[] Vertices { get; set; }

    public uint OriginalU { get; set; }

    public uint OriginalV { get; set; }

    public uint WidthMinusOne { get; set; }

    public uint HeightMinusOne { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(Attribute);
            writer.Write(TileAndFlag);
            writer.Write(NewFlags);
            foreach (TRObjectTextureVert vert in Vertices) { writer.Write(vert.Serialize()); }
            writer.Write(OriginalU);
            writer.Write(OriginalV);
            writer.Write(WidthMinusOne);
            writer.Write(HeightMinusOne);
        }

        return stream.ToArray();
    }
}
