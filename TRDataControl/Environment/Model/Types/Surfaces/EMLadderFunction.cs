using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMLadderFunction : EMRefaceFunction
{
    public EMLocation Location { get; set; }
    public bool IsPositiveX { get; set; }
    public bool IsPositiveZ { get; set; }
    public bool IsNegativeX { get; set; }
    public bool IsNegativeZ { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        var data = GetData(level);
        ModifyLadder(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));

        base.ApplyToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        var data = GetData(level);
        ModifyLadder(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));

        base.ApplyToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        var data = GetData(level);
        ModifyLadder(level.FloorData, loc => level.GetRoomSector(data.ConvertLocation(loc)));

        base.ApplyToLevel(level);
    }

    private void ModifyLadder(FDControl floorData, Func<EMLocation, TRRoomSector> sectorGetter)
    {
        var sector = sectorGetter(Location);
        bool removeAll = !IsPositiveX && !IsPositiveZ && !IsNegativeX && !IsNegativeZ;
        if (removeAll && sector.FDIndex == 0)
        {
            return;
        }

        if (sector.FDIndex == 0)
        {
            floorData.CreateFloorData(sector);
        }

        var entries = floorData[sector.FDIndex];
        if (removeAll)
        {
            entries.RemoveAll(e => e is FDClimbEntry);
            return;
        }

        var ladder = entries.OfType<FDClimbEntry>().FirstOrDefault();
        if (ladder == null)
        {
            ladder = new FDClimbEntry();
            floorData[sector.FDIndex].Add(ladder);
        }

        ladder.IsPositiveX = IsPositiveX;
        ladder.IsPositiveZ = IsPositiveZ;
        ladder.IsNegativeX = IsNegativeX;
        ladder.IsNegativeZ = IsNegativeZ;
    }
}
