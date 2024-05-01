namespace TRLevelControl.Model;

public class TR4Room : TRRoom
{
    public TRRoomMesh<TR4Type, TR3RoomVertex> Mesh { get; set; }
    public TRColour4 Colour { get; set; }
    public List<TR4RoomLight> Lights { get; set; }    
    public List<TR4RoomStaticMesh> StaticMeshes { get; set; }
    public byte WaterScheme { get; set; }
    public TRPSXReverbMode ReverbMode { get; set; }
    public byte AlternateGroup { get; set; }
}
