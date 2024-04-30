using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMoveStaticMeshFunction : BaseEMFunction
{
    public Dictionary<short, Dictionary<int, EMLocation>> Relocations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in Relocations.Keys)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(roomIndex)];
            foreach (int meshIndex in Relocations[roomIndex].Keys)
            {
                MoveMesh(room.StaticMeshes[meshIndex], Relocations[roomIndex][meshIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in Relocations.Keys)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
            foreach (int meshIndex in Relocations[roomIndex].Keys)
            {
                MoveMesh(room.StaticMeshes[meshIndex], Relocations[roomIndex][meshIndex]);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in Relocations.Keys)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
            foreach (int meshIndex in Relocations[roomIndex].Keys)
            {
                MoveMesh(room.StaticMeshes[meshIndex], Relocations[roomIndex][meshIndex]);
            }
        }
    }

    private static void MoveMesh(TRRoomStaticMesh mesh, EMLocation amendment)
    {
        mesh.X += amendment.X;
        mesh.Y += amendment.Y;
        mesh.Z += amendment.Z;
        mesh.Rotation = amendment.Angle;
    }
}
