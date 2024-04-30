namespace TRLevelControl.Model;

public class TR2RoomVertex : TRRoomVertex
{
    public short Lighting { get; set; }
    public ushort Attributes { get; set; }
    public short Lighting2 { get; set; }

    public override TRRoomVertex Clone()
    {
        return new TR2RoomVertex
        {
            Vertex = Vertex.Clone(),
            Lighting = Lighting,
            Attributes = Attributes,
            Lighting2 = Lighting2
        };
    }
}
