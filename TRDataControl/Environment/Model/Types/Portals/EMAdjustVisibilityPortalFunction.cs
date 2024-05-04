using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAdjustVisibilityPortalFunction : BaseEMFunction
{
    public short BaseRoom { get; set; }
    public short AdjoiningRoom { get; set; }
    public Dictionary<int, TRVertex> VertexChanges { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        AdjustPortals(level.Rooms[data.ConvertRoom(BaseRoom)].Portals.Where(p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        AdjustPortals(level.Rooms[data.ConvertRoom(BaseRoom)].Portals.Where(p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        AdjustPortals(level.Rooms[data.ConvertRoom(BaseRoom)].Portals.Where(p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
    }

    private void AdjustPortals(IEnumerable<TRRoomPortal> portals)
    {
        foreach (TRRoomPortal portal in portals)
        {
            foreach (int vertexIndex in VertexChanges.Keys)
            {
                TRVertex currentVertex = portal.Vertices[vertexIndex];
                TRVertex vertexChange = VertexChanges[vertexIndex];
                currentVertex.X += vertexChange.X;
                currentVertex.Y += vertexChange.Y;
                currentVertex.Z += vertexChange.Z;
            }
        }
    }
}
