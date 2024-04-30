namespace TRLevelControl.Model;

public class TR1RoomVertex : TRRoomVertex
{
    public short Lighting { get; set; }

    public override TRRoomVertex Clone()
    {
        return new TR1RoomVertex
        {
            Vertex = Vertex.Clone(),
            Lighting = Lighting
        };
    }
}
