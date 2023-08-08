using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMSwapFaceFunction : BaseEMFunction
{
    public short RoomIndex { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public Dictionary<int, int> Swaps { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        TRRoom room = level.Rooms[data.ConvertRoom(RoomIndex)];
        if (FaceType == EMTextureFaceType.Rectangles)
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.RoomData.Rectangles[targetIndex], room.RoomData.Rectangles[originalIndex])
                    = (room.RoomData.Rectangles[originalIndex], room.RoomData.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.RoomData.Triangles[targetIndex], room.RoomData.Triangles[originalIndex])
                    = (room.RoomData.Triangles[originalIndex], room.RoomData.Triangles[targetIndex]);
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
                (room.RoomData.Rectangles[targetIndex], room.RoomData.Rectangles[originalIndex])
                    = (room.RoomData.Rectangles[originalIndex], room.RoomData.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.RoomData.Triangles[targetIndex], room.RoomData.Triangles[originalIndex])
                    = (room.RoomData.Triangles[originalIndex], room.RoomData.Triangles[targetIndex]);
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
                (room.RoomData.Rectangles[targetIndex], room.RoomData.Rectangles[originalIndex])
                    = (room.RoomData.Rectangles[originalIndex], room.RoomData.Rectangles[targetIndex]);
            }
        }
        else
        {
            foreach (int originalIndex in Swaps.Keys)
            {
                int targetIndex = Swaps[originalIndex];
                // Swap their positions in the list
                (room.RoomData.Triangles[targetIndex], room.RoomData.Triangles[originalIndex])
                    = (room.RoomData.Triangles[originalIndex], room.RoomData.Triangles[targetIndex]);
            }
        }
    }
}
