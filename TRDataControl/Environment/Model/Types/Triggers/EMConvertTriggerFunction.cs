using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMConvertTriggerFunction : BaseEMFunction
{
    public EMLocation Location { get; set; }
    public List<EMLocation> Locations { get; set; }
    public FDTrigType? TrigType { get; set; }
    public bool? OneShot { get; set; }
    public short? SwitchOrKeyRef { get; set; }
    public byte? Mask { get; set; }
    public byte? Timer { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        InitialiseLocations();

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            ConvertTrigger(sector, level.FloorData, data);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        InitialiseLocations();

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            ConvertTrigger(sector, level.FloorData, data);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        InitialiseLocations();

        foreach (EMLocation location in Locations)
        {
            TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
            ConvertTrigger(sector, level.FloorData, data);
        }
    }

    private void InitialiseLocations()
    {
        Locations ??= new();
        if (Location != null)
        {
            // For backwards compatibility with mods already defined - using the List is the preferred way
            Locations.Add(Location);
        }
    }

    private void ConvertTrigger(TRRoomSector sector, FDControl floorData, EMLevelData data)
    {
        if (sector.FDIndex != 0)
        {
            IEnumerable<FDTriggerEntry> triggers = floorData[sector.FDIndex].FindAll(e => e is FDTriggerEntry).Cast<FDTriggerEntry>();
            foreach (FDTriggerEntry trigger in triggers)
            {
                if (TrigType.HasValue)
                {
                    if (trigger.TrigType == FDTrigType.Pickup && TrigType.Value != FDTrigType.Pickup && trigger.Actions.Count > 0)
                    {
                        // The first action entry for pickup triggers is the pickup reference itself, so
                        // this is no longer needed.
                        trigger.Actions.RemoveAt(0);
                    }
                    trigger.TrigType = TrigType.Value;
                }
                if (OneShot.HasValue)
                {
                    trigger.OneShot = OneShot.Value;
                }
                if (SwitchOrKeyRef.HasValue)
                {
                    trigger.SwitchOrKeyRef = data.ConvertEntity(SwitchOrKeyRef.Value);
                }
                if (Mask.HasValue)
                {
                    trigger.Mask = Mask.Value;
                }
                if (Timer.HasValue)
                {
                    trigger.Timer = Timer.Value;
                }
            }
        }
    }
}
