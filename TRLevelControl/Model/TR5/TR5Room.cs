namespace TRLevelControl.Model;

public class TR5Room : TRRoom
{
    public TRRoomMesh<TR5Type, TR5RoomVertex> Mesh { get; set; } = new();
    public TRColour4 Colour { get; set; }
    public List<TR5RoomLight> Lights { get; set; }
    public List<TR5RoomStaticMesh> StaticMeshes { get; set; }
    public TRPSXReverbMode ReverbMode { get; set; }
    public byte AlternateGroup { get; set; }
    public ushort WaterScheme { get; set; }
}
