using System.Collections.Generic;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMFloodFunction : BaseWaterFunction
    {
        public override void ApplyToLevel(TR2Level level)
        {
            // Loop initially to flood everything as our texture checks below involve
            // determining which rooms have water.
            foreach (int roomNumber in RoomNumbers)
            {
                level.Rooms[roomNumber].Fill();
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
                        RemoveWaterSurface(room);
                        RemoveWaterSurface(roomBelow);
                    }
                }

                ISet<byte> roomsAbove = GetAdjacentRooms(room, true);
                foreach (byte roomAboveNumber in roomsAbove)
                {
                    TR2Room roomAbove = level.Rooms[roomAboveNumber];
                    if (!roomAbove.ContainsWater)
                    {
                        AddWaterSurface(level, room, true, new int[] { roomAboveNumber });
                        AddWaterSurface(level, roomAbove, false, RoomNumbers);
                    }
                }
            }
        }
    }
}