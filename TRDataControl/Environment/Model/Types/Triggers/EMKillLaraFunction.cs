using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMKillLaraFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public bool Remove { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AlterDeathTile(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AlterDeathTile(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            AlterDeathTile(sector, level.FloorData);
        }
    }

    private void AlterDeathTile(TRRoomSector sector, FDControl control)
    {
        if (sector.FDIndex == 0 && !Remove)
        {
            control.CreateFloorData(sector);
        }

        if (sector.FDIndex == 0)
        {
            return;
        }

        var entries = control[sector.FDIndex];
        var deathEntry = entries.FirstOrDefault(e => e is FDKillLaraEntry);
        if (Remove)
        {
            if (deathEntry != null)
            {
                entries.Remove(deathEntry);
            }
        }
        else if (deathEntry == null)
        {
            entries.Add(new FDKillLaraEntry());
        }
    }
}
