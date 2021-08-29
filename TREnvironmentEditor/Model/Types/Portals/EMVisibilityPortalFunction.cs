using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMVisibilityPortalFunction : BaseEMFunction
    {
        public Dictionary<int, TRRoomPortal> Portals { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            foreach (int roomNumber in Portals.Keys)
            {
                TR2Room room = level.Rooms[roomNumber];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(Portals[roomNumber]);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }
    }
}