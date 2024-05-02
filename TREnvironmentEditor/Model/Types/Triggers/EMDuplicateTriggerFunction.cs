using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMDuplicateTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public EMLocation BaseLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(BaseLocation));
        if (baseSector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> triggerEntries = level.FloorData[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
        if (triggerEntries.Count == 0)
        {
            return;
        }

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AppendTriggers(sector, triggerEntries, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(BaseLocation));
        if (baseSector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> triggerEntries = level.FloorData[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
        if (triggerEntries.Count == 0)
        {
            return;
        }

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AppendTriggers(sector, triggerEntries, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(BaseLocation));
        if (baseSector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> triggerEntries = level.FloorData[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
        if (triggerEntries.Count == 0)
        {
            return;
        }

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AppendTriggers(sector, triggerEntries, level.FloorData);
        }
    }

    private static void AppendTriggers(TRRoomSector sector, List<FDEntry> triggerEntries, FDControl control)
    {
        if (sector.FDIndex == 0)
        {
            control.CreateFloorData(sector);
        }

        List<FDEntry> entries = control[sector.FDIndex];
        if (entries.FindIndex(e => e is FDTriggerEntry) == -1)
        {
            entries.AddRange(triggerEntries);
        }
    }
}
