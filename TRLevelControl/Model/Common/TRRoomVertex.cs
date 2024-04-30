namespace TRLevelControl.Model;

public abstract class TRRoomVertex
{
    public TRVertex Vertex { get; set; }

    public abstract TRRoomVertex Clone();
}
