namespace TRLevelControl.Model;

public class TRFace : ICloneable
{
    public TRFaceType Type { get; set; } = TRFaceType.Rectangle;
    public List<ushort> Vertices { get; set; }
    public ushort Texture { get; set; }
    public bool DoubleSided { get; set; }
    public bool UnknownFlag { get; set; }

    public void SwapVertices(int pos1, int pos2)
    {
        (Vertices[pos2], Vertices[pos1]) = (Vertices[pos1], Vertices[pos2]);
    }

    public TRFace Clone()
    {
        return new()
        {
            Type = Type,
            Vertices = new(Vertices),
            Texture = Texture,
            DoubleSided = DoubleSided,
            UnknownFlag = UnknownFlag,
        };
    }

    object ICloneable.Clone()
        => Clone();
}
