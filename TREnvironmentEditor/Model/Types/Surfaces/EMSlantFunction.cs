using TREnvironmentEditor.Helpers;
using TRLevelControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMSlantFunction : EMClickFunction
{
    public FDSlantType SlantType { get; set; }
    public sbyte? XSlant { get; set; }
    public sbyte? ZSlant { get; set; }
    public bool RemoveSlant { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        // Apply click changes first
        base.ApplyToLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level);
            UpdateSlantEntry(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        base.ApplyToLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level);
            UpdateSlantEntry(sector, level.FloorData);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        base.ApplyToLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, location.Room, level);
            UpdateSlantEntry(sector, level.FloorData);
        }
    }

    private void UpdateSlantEntry(TRRoomSector sector, FDControl floorData)
    {
        if (RemoveSlant)
        {
            RemoveSlantEntry(sector, floorData);
        }
        else
        {
            CreateSlantEntry(sector, floorData);
        }
    }

    private void RemoveSlantEntry(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> entries = floorData[sector.FDIndex];
        entries.RemoveAll(e => e is FDSlantEntry slant && slant.Type == SlantType);
    }

    private void CreateSlantEntry(TRRoomSector sector, FDControl floorData)
    {
        if (sector.FDIndex == 0)
        {
            floorData.CreateFloorData(sector);
        }

        FDSlantEntry newSlant = new()
        {
            Type = SlantType
        };
        if (XSlant.HasValue)
        {
            newSlant.XSlant = XSlant.Value;
        }
        if (ZSlant.HasValue)
        {
            newSlant.ZSlant = ZSlant.Value;
        }

        List<FDEntry> entries = floorData[sector.FDIndex];

        // Only one slant of each type is supported, and floor must come before ceiling and both before anything else.
        // For ease, remove any existing slants, then re-add/replace as needed.
        FDEntry floorSlant = entries.Find(e => e is FDSlantEntry slant && slant.Type == FDSlantType.Floor);
        FDEntry ceilingSlant = entries.Find(e => e is FDSlantEntry slant && slant.Type == FDSlantType.Ceiling);

        if (floorSlant != null)
        {
            entries.Remove(floorSlant);
        }
        if (ceilingSlant != null)
        {
            entries.Remove(ceilingSlant);
        }

        if (SlantType == FDSlantType.Floor)
        {
            floorSlant = newSlant;
        }
        else
        {
            ceilingSlant = newSlant;
        }

        if (ceilingSlant != null)
        {
            entries.Insert(0, ceilingSlant);
        }
        if (floorSlant != null)
        {
            entries.Insert(0, floorSlant);
        }
    }

    protected override int GetEntityYShift(int clicks)
    {
        List<sbyte> corners = new() { 0, 0, 0, 0 };
        if (XSlant.HasValue && XSlant > 0)
        {
            corners[0] += XSlant.Value;
            corners[1] += XSlant.Value;
        }
        else if (XSlant.HasValue && XSlant < 0)
        {
            corners[2] -= XSlant.Value;
            corners[3] -= XSlant.Value;
        }

        if (ZSlant.HasValue && ZSlant > 0)
        {
            corners[0] += ZSlant.Value;
            corners[2] += ZSlant.Value;
        }
        else if (ZSlant.HasValue && ZSlant < 0)
        {
            corners[1] -= ZSlant.Value;
            corners[3] -= ZSlant.Value;
        }

        // Half-way down the slope
        return (clicks * TRConsts.Step1) + (corners.Max() - corners.Min()) * TRConsts.Step1 / 2;
    }
}
