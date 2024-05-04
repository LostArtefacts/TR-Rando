using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMSwapFaceFunction : BaseEMFunction
{
    public short RoomIndex { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public Dictionary<int, int> Swaps { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        TR1Room room = level.Rooms[data.ConvertRoom(RoomIndex)];
        if (FaceType == EMTextureFaceType.Rectangles)
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Rectangles[targetIndex], room.Mesh.Rectangles[originalIndex])
                    = (room.Mesh.Rectangles[originalIndex], room.Mesh.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Triangles[targetIndex], room.Mesh.Triangles[originalIndex])
                    = (room.Mesh.Triangles[originalIndex], room.Mesh.Triangles[targetIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        TR2Room room = level.Rooms[data.ConvertRoom(RoomIndex)];
        if (FaceType == EMTextureFaceType.Rectangles)
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Rectangles[targetIndex], room.Mesh.Rectangles[originalIndex])
                    = (room.Mesh.Rectangles[originalIndex], room.Mesh.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Triangles[targetIndex], room.Mesh.Triangles[originalIndex])
                    = (room.Mesh.Triangles[originalIndex], room.Mesh.Triangles[targetIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        TR3Room room = level.Rooms[data.ConvertRoom(RoomIndex)];
        if (FaceType == EMTextureFaceType.Rectangles)
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Rectangles[targetIndex], room.Mesh.Rectangles[originalIndex])
                    = (room.Mesh.Rectangles[originalIndex], room.Mesh.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.Mesh.Triangles[targetIndex], room.Mesh.Triangles[originalIndex])
                    = (room.Mesh.Triangles[originalIndex], room.Mesh.Triangles[targetIndex]);
            }
        }
    }
}
