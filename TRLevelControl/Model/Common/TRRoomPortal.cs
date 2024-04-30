namespace TRLevelControl.Model;

public class TRRoomPortal
{
    public ushort AdjoiningRoom { get; set; }
    public TRVertex Normal { get; set; }
    public List<TRVertex> Vertices { get; set; }
}
