namespace TRLevelControl.Model;

public class TR3Room : TRRoom
{
    public TRRoomMesh<TR3Type, TR3RoomVertex> Mesh { get; set; }
    public short AmbientIntensity { get; set; }
    public short LightMode { get; set; }
    public List<TR3RoomLight> Lights { get; set; }
    public List<TR3RoomStaticMesh> StaticMeshes { get; set; }
    public byte WaterScheme { get; set; }
    public byte ReverbInfo { get; set; }
    public byte Filler { get; set; }
}
