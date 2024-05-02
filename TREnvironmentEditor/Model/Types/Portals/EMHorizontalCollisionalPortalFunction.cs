using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMHorizontalCollisionalPortalFunction : BaseEMFunction
{
    public Dictionary<short, Dictionary<short, EMLocation[]>> Portals { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        Dictionary<TRRoomSector, List<short>> sectorMap = new();

        foreach (short fromRoomNumber in Portals.Keys)
        {
            short convertedFromRoomNumber = data.ConvertRoom(fromRoomNumber);
            foreach (short toRoomNumber in Portals[fromRoomNumber].Keys)
            {
                short convertedToRoomNumber = data.ConvertRoom(toRoomNumber);
                foreach (EMLocation sectorLocation in Portals[fromRoomNumber][toRoomNumber])
                {
                    TRRoomSector sector = level.FloorData.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level);

                    if (!sectorMap.ContainsKey(sector))
                    {
                        sectorMap[sector] = new();
                    }
                    sectorMap[sector].Add(convertedToRoomNumber);
                }
            }
        }

        CreatePortals(sectorMap, level.FloorData);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        Dictionary<TRRoomSector, List<short>> sectorMap = new();

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
                    TRRoomSector sector = level.FloorData.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level);

                    if (!sectorMap.ContainsKey(sector))
                    {
                        sectorMap[sector] = new();
                    }
                    sectorMap[sector].Add(convertedToRoomNumber);
                }
            }
        }

        CreatePortals(sectorMap, level.FloorData);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        Dictionary<TRRoomSector, List<short>> sectorMap = new();

        foreach (short fromRoomNumber in Portals.Keys)
        {
            short convertedFromRoomNumber = data.ConvertRoom(fromRoomNumber);
            foreach (short toRoomNumber in Portals[fromRoomNumber].Keys)
            {
                short convertedToRoomNumber = data.ConvertRoom(toRoomNumber);
                foreach (EMLocation sectorLocation in Portals[fromRoomNumber][toRoomNumber])
                {
                    TRRoomSector sector = level.FloorData.GetRoomSector(sectorLocation.X, sectorLocation.Y, sectorLocation.Z, convertedFromRoomNumber, level);

                    if (!sectorMap.ContainsKey(sector))
                    {
                        sectorMap[sector] = new();
                    }
                    sectorMap[sector].Add(convertedToRoomNumber);
                }
            }
        }

        CreatePortals(sectorMap, level.FloorData);
    }

    private static void CreatePortals(Dictionary<TRRoomSector, List<short>> sectorMap, FDControl control)
    {
        foreach (TRRoomSector sector in sectorMap.Keys)
        {
            if (sector.FDIndex == 0)
            {
                control.CreateFloorData(sector);
            }

            foreach (short roomNumber in sectorMap[sector])
            {
                control[sector.FDIndex].Add(new FDPortalEntry
                {
                    Room = roomNumber
                });
            }
        }
    }
}
