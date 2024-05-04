using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRemoveTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public List<int> Rooms { get; set; }
    public List<FDTrigType> TrigTypes { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                RemoveSectorTriggers(sector, level.FloorData);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].Sectors, level.FloorData);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                RemoveSectorTriggers(sector, level.FloorData);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].Sectors, level.FloorData);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                RemoveSectorTriggers(sector, level.FloorData);
            }
        }

        if (Rooms != null)
        {
            foreach (int roomNumber in Rooms)
            {
                RemoveSectorListTriggers(level.Rooms[data.ConvertRoom(roomNumber)].Sectors, level.FloorData);
            }
        }
    }

    private void RemoveSectorListTriggers(IEnumerable<TRRoomSector> sectors, FDControl control)
    {
        foreach (TRRoomSector sector in sectors)
        {
            RemoveSectorTriggers(sector, control);
        }
    }

    private void RemoveSectorTriggers(TRRoomSector sector, FDControl control)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> entries = control[sector.FDIndex];
        entries.RemoveAll(e => e is FDTriggerEntry trig && (TrigTypes?.Contains(trig.TrigType) ?? true));
    }
}
