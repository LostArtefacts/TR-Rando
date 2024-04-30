namespace TRLevelControl.Model;

public class TRRoomMesh<T, V>
    where T : Enum
    where V : TRRoomVertex
{
    public List<V> Vertices { get; set; }
    public List<TRFace> Rectangles { get; set; }
    public List<TRFace> Triangles { get; set; }
    public List<TRRoomSprite<T>> Sprites { get; set; }

    public IEnumerable<TRFace> Faces => Rectangles.Concat(Triangles);
}
