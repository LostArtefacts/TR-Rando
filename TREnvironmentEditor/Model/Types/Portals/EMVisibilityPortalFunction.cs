using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMVisibilityPortalFunction : BaseEMFunction
    {
        public List<EMVisibilityPortal> Portals {get; set;}

        public override void ApplyToLevel(TR1Level level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };

            foreach (EMVisibilityPortal emPortal in Portals)
            {
                TRRoomPortal portal = emPortal.ToPortal(data);
                TRRoom room = level.Rooms[emPortal.BaseRoom];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(portal);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };

            foreach (EMVisibilityPortal emPortal in Portals)
            {
                TRRoomPortal portal = emPortal.ToPortal(data);
                TR2Room room = level.Rooms[emPortal.BaseRoom];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(portal);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };

            foreach (EMVisibilityPortal emPortal in Portals)
            {
                TRRoomPortal portal = emPortal.ToPortal(data);
                TR3Room room = level.Rooms[emPortal.BaseRoom];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(portal);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }
    }
}