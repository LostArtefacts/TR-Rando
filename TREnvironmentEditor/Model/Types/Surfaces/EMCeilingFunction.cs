using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMCeilingFunction : BaseEMFunction
    {
        public Dictionary<int, sbyte> CeilingHeights { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            foreach (int roomNumber in CeilingHeights.Keys)
            {
                TRRoom room = level.Rooms[data.ConvertRoom(roomNumber)];
                int min = room.Info.YTop / ClickSize;
                foreach (TRRoomSector sector in room.Sectors)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
                room.Info.YTop = min * ClickSize;
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            foreach (int roomNumber in CeilingHeights.Keys)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                int min = room.Info.YTop / ClickSize;
                foreach (TRRoomSector sector in room.SectorList)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
                room.Info.YTop = min * ClickSize;
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            foreach (int roomNumber in CeilingHeights.Keys)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
                int min = room.Info.YTop / ClickSize;
                foreach (TRRoomSector sector in room.Sectors)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
                room.Info.YTop = min * ClickSize;
            }
        }
    }
}