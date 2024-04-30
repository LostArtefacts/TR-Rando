namespace TRLevelControl.Model;

public class TR1Room
{
    public TRRoomInfo Info { get; set; }
    public TRRoomMesh<TR1Type, TR1RoomVertex> Mesh { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AmbientIntensity { get; set; }
    public List<TR1RoomLight> Lights { get; set; }
    public List<TR1RoomStaticMesh> StaticMeshes { get; set; }
    public short AlternateRoom { get; set; }
    public short Flags { get; set; }

    public bool ContainsWater
    {
        get => (Flags & 0x01) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x01;
            }
            else
            {
                Flags &= ~0x01;
            }
        }
    }
 }
