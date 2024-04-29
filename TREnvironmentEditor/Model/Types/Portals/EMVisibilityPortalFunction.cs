using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMVisibilityPortalFunction : BaseEMFunction
{
    public List<EMVisibilityPortal> Portals {get; set;}

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMVisibilityPortal emPortal in Portals)
        {
            TRRoomPortal portal = emPortal.ToPortal(data);
            level.Rooms[emPortal.BaseRoom].Portals.Add(portal);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMVisibilityPortal emPortal in Portals)
        {
            TRRoomPortal portal = emPortal.ToPortal(data);
            level.Rooms[emPortal.BaseRoom].Portals.Add(portal);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMVisibilityPortal emPortal in Portals)
        {
            TRRoomPortal portal = emPortal.ToPortal(data);
            level.Rooms[emPortal.BaseRoom].Portals.Add(portal);
        }
    }
}
