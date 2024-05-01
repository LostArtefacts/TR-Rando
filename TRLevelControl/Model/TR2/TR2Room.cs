namespace TRLevelControl.Model;

public class TR2Room : TRRoom
{
    public TRRoomMesh<TR2Type, TR2RoomVertex> Mesh { get; set; }
    public short AmbientIntensity { get; set; }
    public short AmbientIntensity2 { get; set; }
    public short LightMode { get; set; }
    public List<TR2RoomLight> Lights { get; set; }
    public List<TR2RoomStaticMesh> StaticMeshes { get; set; }
}
