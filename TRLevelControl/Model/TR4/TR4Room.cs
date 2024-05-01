namespace TRLevelControl.Model;

public class TR4Room : TRRoom
{
    public TRRoomMesh<TR4Type, TR3RoomVertex> Mesh { get; set; }
    public short AmbientIntensity { get; set; }
    public short LightMode { get; set; }
    public List<TR4RoomLight> Lights { get; set; }    
    public List<TR4RoomStaticMesh> StaticMeshes { get; set; }
    public byte WaterScheme { get; set; }
    public byte ReverbInfo { get; set; }
    public byte Filler { get; set; }
}
