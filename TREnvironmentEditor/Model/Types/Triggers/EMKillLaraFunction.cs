using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMKillLaraFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            CreateTrigger(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            CreateTrigger(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            CreateTrigger(sector, level.FloorData);
        }
    }

    private static void CreateTrigger(TRRoomSector sector, FDControl control)
    {
        // If there is no floor data create the FD to begin with.
        if (sector.FDIndex == 0)
        {
            control.CreateFloorData(sector);
        }

        List<FDEntry> entries = control[sector.FDIndex];
        if (entries.FindIndex(e => e is FDKillLaraEntry) == -1)
        {
            entries.Add(new FDKillLaraEntry());
        }
    }
}
