namespace TRLevelControl.Model;

public class TRSoundSource<T>
    where T : Enum
{
    public T ID { get; set; }
    public TRSoundMode Mode { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
}
