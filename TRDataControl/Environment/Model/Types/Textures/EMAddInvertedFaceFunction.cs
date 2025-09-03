using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddInvertedFaceFunction : BaseEMFunction
{
    public Dictionary<int, List<int>> RoomFaces { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        foreach (var (room, faceIds) in RoomFaces)
        {
            AddInvertedFaces(level.Rooms[room].Mesh, faceIds);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        foreach (var (room, faceIds) in RoomFaces)
        {
            AddInvertedFaces(level.Rooms[room].Mesh, faceIds);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        foreach (var (room, faceIds) in RoomFaces)
        {
            AddInvertedFaces(level.Rooms[room].Mesh, faceIds);
        }
    }

    private static void AddInvertedFaces<T, V>(TRRoomMesh<T, V> mesh, List<int> faceIds)
        where T : Enum
        where V : TRRoomVertex
    {
        mesh.Rectangles.AddRange(faceIds.Select(i =>
        {
            var face = mesh.Rectangles[i].Clone();
            face.SwapVertices(0, 1);
            face.SwapVertices(2, 3);
            return face;
        }));
    }
}
