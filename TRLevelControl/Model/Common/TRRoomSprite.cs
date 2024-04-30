namespace TRLevelControl.Model;

public class TRRoomSprite<T>
    where T : Enum
{
    public T ID { get; set; }
    public short Vertex { get; set; }
}
