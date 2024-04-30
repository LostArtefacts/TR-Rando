namespace TRLevelControl.Model;

public class TR4Room
{
    public TRRoomInfo Info { get; set; }
    public TRRoomMesh<TR3RoomVertex> Mesh { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AmbientIntensity { get; set; }
    public short LightMode { get; set; }
    public List<TR4RoomLight> Lights { get; set; }    
    public List<TR4RoomStaticMesh> StaticMeshes { get; set; }
    public short AlternateRoom { get; set; }
    public short Flags { get; set; }
    public byte WaterScheme { get; set; }
    public byte ReverbInfo { get; set; }
    public byte Filler { get; set; }
}
