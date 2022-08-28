using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMHorizontalCollisionalPortalFunction : BaseEMFunction
    {
        public Dictionary<short, Dictionary<short, EMLocation[]>> Portals { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            Dictionary<TRRoomSector, List<ushort>> sectorMap = new Dictionary<TRRoomSector, List<ushort>>();

            foreach (short fromRoomNumber in Portals.Keys)
            {
                short convertedFromRoomNumber = data.ConvertRoom(fromRoomNumber);
                foreach (short toRoomNumber in Portals[fromRoomNumber].Keys)
                {
                    short convertedToRoomNumber = data.ConvertRoom(toRoomNumber);
                    foreach (EMLocation sectorLocation in Portals[fromRoomNumber][toRoomNumber])
                    {
                        TRRoomSector sector = FDUtilities.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level, control);

                        if (!sectorMap.ContainsKey(sector))
                        {
                            sectorMap[sector] = new List<ushort>();
                        }
                        sectorMap[sector].Add((ushort)convertedToRoomNumber);
                    }
                }
            }

            CreatePortals(sectorMap, control);

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            // Given a room number, we want to create collisional portals into the other room
            // using the given locations to find the correct sector.
            // See 4.4.1 in https://opentomb.github.io/TRosettaStone3/trosettastone.html#_floordata_functions

            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            Dictionary<TRRoomSector, List<ushort>> sectorMap = new Dictionary<TRRoomSector, List<ushort>>();

            // Because some sectors may be shared, we need to call GetRoomSector to get all the sectors we are
            // interested in first before making any changes.
            foreach (short fromRoomNumber in Portals.Keys)
            {
                short convertedFromRoomNumber = data.ConvertRoom(fromRoomNumber);
                foreach (short toRoomNumber in Portals[fromRoomNumber].Keys)
                {
                    short convertedToRoomNumber = data.ConvertRoom(toRoomNumber);
                    foreach (EMLocation sectorLocation in Portals[fromRoomNumber][toRoomNumber])
                    {
                        TRRoomSector sector = FDUtilities.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level, control);

                        if (!sectorMap.ContainsKey(sector))
                        {
                            sectorMap[sector] = new List<ushort>();
                        }
                        sectorMap[sector].Add((ushort)convertedToRoomNumber);
                    }
                }
            }

            // Now create the new entries for all the portals.
            CreatePortals(sectorMap, control);

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            Dictionary<TRRoomSector, List<ushort>> sectorMap = new Dictionary<TRRoomSector, List<ushort>>();

            foreach (short fromRoomNumber in Portals.Keys)
            {
                short convertedFromRoomNumber = data.ConvertRoom(fromRoomNumber);
                foreach (short toRoomNumber in Portals[fromRoomNumber].Keys)
                {
                    short convertedToRoomNumber = data.ConvertRoom(toRoomNumber);
                    foreach (EMLocation sectorLocation in Portals[fromRoomNumber][toRoomNumber])
                    {
                        TRRoomSector sector = FDUtilities.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level, control);

                        if (!sectorMap.ContainsKey(sector))
                        {
                            sectorMap[sector] = new List<ushort>();
                        }
                        sectorMap[sector].Add((ushort)convertedToRoomNumber);
                    }
                }
            }

            CreatePortals(sectorMap, control);

            control.WriteToLevel(level);
        }

        private void CreatePortals(Dictionary<TRRoomSector, List<ushort>> sectorMap, FDControl control)
        {
            foreach (TRRoomSector sector in sectorMap.Keys)
            {
                if (sector.FDIndex == 0)
                {
                    control.CreateFloorData(sector);
                }

                foreach (ushort roomNumber in sectorMap[sector])
                {
                    control.Entries[sector.FDIndex].Add(new FDPortalEntry
                    {
                        Setup = new FDSetup { Value = 32769 },
                        Room = roomNumber
                    });
                }
            }
        }
    }
}