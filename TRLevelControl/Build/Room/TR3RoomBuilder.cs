using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR3RoomBuilder : TRRoomBuilder<TR3Type, TR3RoomVertex>
{
    public TR3RoomBuilder()
        : base(TRGameVersion.TR3) { }

    protected override List<TR3RoomVertex> ReadVertices(TRLevelReader reader)
    {
        short numVertices = reader.ReadInt16();
        List<TR3RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
                Attributes = reader.ReadUInt16(),
                Colour = reader.ReadUInt16(),
            });
        }
        return vertices;
    }

    protected override void WriteVertices(TRLevelWriter writer, List<TR3RoomVertex> vertices)
    {
        writer.Write((short)vertices.Count);
        foreach (TR3RoomVertex vertex in vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
            writer.Write(vertex.Attributes);
            writer.Write(vertex.Colour);
        }
    }
}
