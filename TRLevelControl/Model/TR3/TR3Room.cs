namespace TRLevelControl.Model;

public class TR3Room
{
    public TRRoomInfo Info { get; set; }
    public TRRoomMesh<TR3Type, TR3RoomVertex> Mesh { get; set; }
    public List<TRRoomPortal> Portals { get; set; }
    public ushort NumZSectors { get; set; }
    public ushort NumXSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public short AmbientIntensity { get; set; }
    public short LightMode { get; set; }
    public List<TR3RoomLight> Lights { get; set; }
    public List<TR3RoomStaticMesh> StaticMeshes { get; set; }
    public short AlternateRoom { get; set; }
    public short Flags { get; set; }
    public byte WaterScheme { get; set; }
    public byte ReverbInfo { get; set; }
    public byte Filler { get; set; }

    public bool ContainsWater
    {
        get
        {
            return (Flags & 0x01) > 0;
        }
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

    public bool IsSkyboxVisible
    {
        get => (Flags & 0x08) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x08;
            }
            else
            {
                Flags &= ~0x08;
            }
        }
    }

    public bool IsWindy
    {
        get => (Flags & 0x20) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x20;
            }
            else
            {
                Flags &= ~0x20;
            }
        }
    }

    public bool IsSwamp
    {
        get => (Flags & 0x80) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x80;
            }
            else
            {
                Flags &= ~0x80;
            }
        }
    }
}
