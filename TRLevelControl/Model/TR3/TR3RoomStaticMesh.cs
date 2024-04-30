namespace TRLevelControl.Model;

public class TR3RoomStaticMesh
{
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Z { get; set; }
    public ushort Rotation { get; set; }
    public ushort Colour { get; set; }
    public ushort Unused { get; set; }
    public ushort MeshID { get; set; }
}
