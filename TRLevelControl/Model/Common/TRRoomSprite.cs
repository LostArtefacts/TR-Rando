namespace TRLevelControl.Model;

public class TRRoomSprite<T> : ICloneable
    where T : Enum
{
    public T ID { get; set; }
    public short Frame { get; set; }
    public short Vertex { get; set; }

    public TRRoomSprite<T> Clone()
        => (TRRoomSprite<T>)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
