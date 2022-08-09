using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMFloodFunction : BaseWaterFunction
    {
        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            // Loop initially to flood everything as our texture checks below involve
            // determining which rooms have water.
            foreach (int roomNumber in RoomNumbers)
            {
                level.Rooms[data.ConvertRoom(roomNumber)].Fill();
            }

            // Work out rooms above and below and what needs water textures as ceilings
            // and what needs them as floors.
            foreach (int roomNumber in RoomNumbers)
            {
                TRRoom room = level.Rooms[data.ConvertRoom(roomNumber)];

                ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
                foreach (byte roomBelowNumber in roomsBelow)
                {
                    TRRoom roomBelow = level.Rooms[roomBelowNumber];
                    if (roomBelow.ContainsWater)
                    {
                        RemoveWaterSurface(room);
                        RemoveWaterSurface(roomBelow);
                    }
                }

                ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
                foreach (byte roomAboveNumber in roomsAbove)
                {
                    TRRoom roomAbove = level.Rooms[roomAboveNumber];
                    if (!roomAbove.ContainsWater)
                    {
                        AddWaterSurface(room, true, new int[] { roomAboveNumber });
                        AddWaterSurface(roomAbove, false, RoomNumbers);
                    }
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            // Loop initially to flood everything as our texture checks below involve
            // determining which rooms have water.
            foreach (int roomNumber in RoomNumbers)
            {
                level.Rooms[data.ConvertRoom(roomNumber)].Fill();
            }

            // Work out rooms above and below and what needs water textures as ceilings
            // and what needs them as floors.
            foreach (int roomNumber in RoomNumbers)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];

                ISet<byte> roomsBelow = GetAdjacentRooms(room.SectorList, false);
                foreach (byte roomBelowNumber in roomsBelow)
                {
                    TR2Room roomBelow = level.Rooms[roomBelowNumber];
                    if (roomBelow.ContainsWater)
                    {
                        RemoveWaterSurface(room);
                        RemoveWaterSurface(roomBelow);
                    }
                }

                ISet<byte> roomsAbove = GetAdjacentRooms(room.SectorList, true);
                foreach (byte roomAboveNumber in roomsAbove)
                {
                    TR2Room roomAbove = level.Rooms[roomAboveNumber];
                    if (!roomAbove.ContainsWater)
                    {
                        AddWaterSurface(room, true, new int[] { roomAboveNumber });
                        AddWaterSurface(roomAbove, false, RoomNumbers);
                    }
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (int roomNumber in RoomNumbers)
            {
                level.Rooms[data.ConvertRoom(roomNumber)].ContainsWater = true;
            }

            foreach (int roomNumber in RoomNumbers)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];

                ISet<byte> roomsBelow = GetAdjacentRooms(room.Sectors, false);
                foreach (byte roomBelowNumber in roomsBelow)
                {
                    TR3Room roomBelow = level.Rooms[roomBelowNumber];
                    if (roomBelow.ContainsWater)
                    {
                        RemoveWaterSurface(room);
                    }
                }

                ISet<byte> roomsAbove = GetAdjacentRooms(room.Sectors, true);
                foreach (byte roomAboveNumber in roomsAbove)
                {
                    TR3Room roomAbove = level.Rooms[roomAboveNumber];
                    if (!roomAbove.ContainsWater)
                    {
                        AddWaterSurface(room, true, new int[] { roomAboveNumber }, floorData);
                        AddWaterSurface(roomAbove, false, RoomNumbers, floorData);
                    }
                }
            }
        }
    }
}