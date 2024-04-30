namespace TRLevelControl.Model;

public class TRRoomMesh<T, V>
    where T : Enum
    where V : TRRoomVertex
{
    public List<V> Vertices { get; set; }
    public List<TRFace4> Rectangles { get; set; }
    public List<TRFace3> Triangles { get; set; }
    public List<TRRoomSprite<T>> Sprites { get; set; }
}
