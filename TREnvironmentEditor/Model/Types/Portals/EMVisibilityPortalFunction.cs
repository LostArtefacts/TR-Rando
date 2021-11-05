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
                TRRoomPortal portal = Portals[roomNumber];
                portal.AdjoiningRoom = (ushort)ConvertItemNumber(portal.AdjoiningRoom, level.NumRooms);

                int convertedRoomNumbew = ConvertItemNumber(roomNumber, level.NumRooms);
                TR2Room room = level.Rooms[convertedRoomNumbew];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(portal);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            foreach (int roomNumber in Portals.Keys)
            {
                TRRoomPortal portal = Portals[roomNumber];
                portal.AdjoiningRoom = (ushort)ConvertItemNumber(portal.AdjoiningRoom, level.NumRooms);

                int convertedRoomNumbew = ConvertItemNumber(roomNumber, level.NumRooms);
                TR3Room room = level.Rooms[convertedRoomNumbew];
                List<TRRoomPortal> portals = room.Portals.ToList();
                portals.Add(portal);
                room.Portals = portals.ToArray();
                room.NumPortals++;
            }
        }
    }
}