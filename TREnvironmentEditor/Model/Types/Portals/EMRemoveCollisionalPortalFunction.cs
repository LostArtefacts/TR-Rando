using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMRemoveCollisionalPortalFunction : BaseEMFunction
    {
        public EMLocation Location1 { get; set; }
        public EMLocation Location2 { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            Location1.Room = data.ConvertRoom(Location1.Room);
            Location2.Room = data.ConvertRoom(Location2.Room);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector1 = FDUtilities.GetRoomSector(Location1.X, Location1.Y, Location1.Z, Location1.Room, level, floorData);
            TRRoomSector sector2 = FDUtilities.GetRoomSector(Location2.X, Location2.Y, Location2.Z, Location2.Room, level, floorData);

            RemovePortals(sector1, sector2, floorData);

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            Location1.Room = data.ConvertRoom(Location1.Room);
            Location2.Room = data.ConvertRoom(Location2.Room);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector1 = FDUtilities.GetRoomSector(Location1.X, Location1.Y, Location1.Z, Location1.Room, level, floorData);
            TRRoomSector sector2 = FDUtilities.GetRoomSector(Location2.X, Location2.Y, Location2.Z, Location2.Room, level, floorData);

            RemovePortals(sector1, sector2, floorData);

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            Location1.Room = data.ConvertRoom(Location1.Room);
            Location2.Room = data.ConvertRoom(Location2.Room);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            TRRoomSector sector1 = FDUtilities.GetRoomSector(Location1.X, Location1.Y, Location1.Z, Location1.Room, level, floorData);
            TRRoomSector sector2 = FDUtilities.GetRoomSector(Location2.X, Location2.Y, Location2.Z, Location2.Room, level, floorData);

            RemovePortals(sector1, sector2, floorData);

            floorData.WriteToLevel(level);
        }

        private void RemovePortals(TRRoomSector sector1, TRRoomSector sector2, FDControl floorData)
        {
            if (sector1 == sector2)
            {
                return;
            }

            RemoveVerticalPortals(sector1);
            RemoveVerticalPortals(sector2);
            RemoveHorizontalPortals(sector1, floorData);
            RemoveHorizontalPortals(sector2, floorData);
        }

        private void RemoveVerticalPortals(TRRoomSector sector)
        {
            if (sector.RoomBelow == Location1.Room || sector.RoomBelow == Location2.Room)
            {
                sector.RoomBelow = 255;
            }
            if (sector.RoomAbove == Location1.Room || sector.RoomAbove == Location2.Room)
            {
                sector.RoomAbove = 255;
            }
        }

        private void RemoveHorizontalPortals(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex == 0)
            {
                return;
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            entries.RemoveAll(e => e is FDPortalEntry portal && (portal.Room == Location1.Room || portal.Room == Location2.Room));
            if (entries.Count == 0)
            {
                floorData.RemoveFloorData(sector);
            }
        }
    }
}
