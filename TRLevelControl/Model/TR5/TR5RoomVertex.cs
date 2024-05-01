namespace TRLevelControl.Model;

public class TR5RoomVertex : TRRoomVertex
{
    public TR5Vertex Normal { get; set; }
    public TRColour4 Colour { get; set; }

    public override TRRoomVertex Clone()
    {
        return new TR5RoomVertex
        {
            Vertex = Vertex.Clone(),
            Normal = Normal.Clone(),
            Colour = Colour.Clone(),
        };
    }
}
