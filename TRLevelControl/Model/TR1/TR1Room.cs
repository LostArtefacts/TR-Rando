namespace TRLevelControl.Model;

public class TR1Room : TRRoom
{
    public TRRoomMesh<TR1Type, TR1RoomVertex> Mesh { get; set; }
    public short AmbientIntensity { get; set; }
    public List<TR1RoomLight> Lights { get; set; }
    public List<TR1RoomStaticMesh> StaticMeshes { get; set; }
 }
