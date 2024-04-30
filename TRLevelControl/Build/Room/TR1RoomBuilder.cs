using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR1RoomBuilder : TRRoomBuilder<TR1RoomVertex>
{
    public TR1RoomBuilder()
        : base(TRGameVersion.TR1) { }

    protected override List<TR1RoomVertex> ReadVertices(TRLevelReader reader)
    {
        short numVertices = reader.ReadInt16();
        List<TR1RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
            });
        }
        return vertices;
    }

    protected override void WriteVertices(TRLevelWriter writer, List<TR1RoomVertex> vertices)
    {
        writer.Write((short)vertices.Count);
        foreach (TR1RoomVertex vertex in vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
        }
    }
}
