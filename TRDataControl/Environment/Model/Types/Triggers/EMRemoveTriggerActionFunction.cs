using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRemoveTriggerActionFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public EMTriggerAction ActionItem { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(location));
            RemoveAction(baseSector, level.FloorData, action);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(location));
            RemoveAction(baseSector, level.FloorData, action);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        foreach (EMLocation location in Locations)
        {
            TRRoomSector baseSector = level.GetRoomSector(data.ConvertLocation(location));
            RemoveAction(baseSector, level.FloorData, action);
        }
    }

    private FDActionItem InitialiseActionItem(EMLevelData data)
    {
        return ActionItem.ToFDAction(data);
    }

    private static void RemoveAction(TRRoomSector sector, FDControl control, FDActionItem action)
    {
        if (sector.FDIndex == 0)
        {
            return;
        }

        List<FDEntry> entries = control[sector.FDIndex].FindAll(e => e is FDTriggerEntry);
        foreach (FDEntry entry in entries)
        {
            (entry as FDTriggerEntry).Actions.RemoveAll(a => a.Action == action.Action && a.Parameter == action.Parameter);
        }
    }
}
