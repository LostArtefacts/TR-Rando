namespace TRLevelControl.Model;

public class TRRoomMesh<V>
    where V : TRRoomVertex
{
    public List<V> Vertices { get; set; }
    public List<TRFace4> Rectangles { get; set; }
    public List<TRFace3> Triangles { get; set; }
    public List<TRRoomSprite> Sprites { get; set; }
}
