namespace TRLevelControl.Model;

public abstract class TR5RoomLight
{
    public abstract TR4RoomLightType Type { get; }
    public TR5Vertex Position { get; set; }
    public TR5Colour Colour { get; set; }
}
