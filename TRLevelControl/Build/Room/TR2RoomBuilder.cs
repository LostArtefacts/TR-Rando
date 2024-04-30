using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR2RoomBuilder : TRRoomBuilder<TR2Type, TR2RoomVertex>
{
    public TR2RoomBuilder()
        : base(TRGameVersion.TR2) { }

    protected override List<TR2RoomVertex> ReadVertices(TRLevelReader reader)
    {
        short numVertices = reader.ReadInt16();
        List<TR2RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
                Attributes = reader.ReadUInt16(),
                Lighting2 = reader.ReadInt16(),
            });
        }
        return vertices;
    }

    protected override void WriteVertices(TRLevelWriter writer, List<TR2RoomVertex> vertices)
    {
        writer.Write((short)vertices.Count);
        foreach (TR2RoomVertex vertex in vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
            writer.Write(vertex.Attributes);
            writer.Write(vertex.Lighting2);
        }
    }
}
