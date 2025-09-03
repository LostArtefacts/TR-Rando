using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMCopyVertexFunction : BaseEMFunction
{
    public TRVertex Shift { get; set; }
    public Dictionary<int, List<int>> RoomVertices { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        foreach (var (room, vertexIds) in RoomVertices)
        {
            CopyVertices(level.Rooms[room].Mesh, vertexIds);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        foreach (var (room, vertexIds) in RoomVertices)
        {
            CopyVertices(level.Rooms[room].Mesh, vertexIds);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        foreach (var (room, vertexIds) in RoomVertices)
        {
            CopyVertices(level.Rooms[room].Mesh, vertexIds);
        }
    }

    private void CopyVertices<T, V>(TRRoomMesh<T, V> mesh, List<int> vertexIds)
        where T : Enum
        where V : TRRoomVertex
    {
        mesh.Vertices.AddRange(vertexIds.Select(i =>
        {
            var vertex = (V)mesh.Vertices[i].Clone();
            vertex.Vertex.X += Shift.X;
            vertex.Vertex.Y += Shift.Y;
            vertex.Vertex.Z += Shift.Z;
            return vertex;
        }));
    }
}
