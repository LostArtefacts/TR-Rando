namespace TRLevelControl.Model;

public class TR5Vertex : ICloneable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public TR5Vertex Clone()
        => (TR5Vertex)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
