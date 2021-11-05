using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMDrainFunction : BaseWaterFunction
    {
        public int[] DetachedRooms { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            // Loop initially to drain everything as our texture checks below involve
            // determining which rooms have water.
            foreach (int roomNumber in RoomNumbers)
            {
                level.Rooms[roomNumber].Drain();
            }

            // Work out rooms above and below and what needs water textures as ceilings
            // and what needs them as floors.
            foreach (int roomNumber in RoomNumbers)
            {
                TR2Room room = level.Rooms[roomNumber];

                ISet<byte> roomsBelow = GetAdjacentRooms(room, false);
                foreach (byte roomBelowNumber in roomsBelow)
                {
                    TR2Room roomBelow = level.Rooms[roomBelowNumber];
                    if (roomBelow.ContainsWater)
                    {
                        AddWaterSurface(level, room, false, RoomNumbers);
                        AddWaterSurface(level, roomBelow, true, RoomNumbers);
                    }
                }

                ISet<byte> roomsAbove = GetAdjacentRooms(room, true);
                foreach (byte roomAboveNumber in roomsAbove)
                {
                    TR2Room roomAbove = level.Rooms[roomAboveNumber];
                    if (!roomAbove.ContainsWater)
                    {
                        RemoveWaterSurface(room);
                        RemoveWaterSurface(roomAbove);
                    }
                }
            }

            // Something odd happens with flip map rooms, e.g. 73/134 in Bartoli's.
            // The 2 rooms don't seem to reference each other, so the textures remain,
            // in the flipped room, even though removed from the normal. For now, 
            // we'll define these rooms manually.
            if (DetachedRooms != null)
            {
                foreach (int roomNumber in DetachedRooms)
                {
                    RemoveWaterSurface(level.Rooms[roomNumber]);
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            throw new System.NotImplementedException();
        }
    }
}