namespace TRLevelControl.Model;

public class TRRoomStaticMesh<T>
    where T : Enum
{
    public T ID { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Angle { get; set; }
}
