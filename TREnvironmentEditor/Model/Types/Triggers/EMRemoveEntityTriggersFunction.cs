using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public class EMRemoveEntityTriggersFunction : BaseEMFunction
{
    public List<int> Entities { get; set; }
    public List<FDTrigType> ExcludedTypes { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        List<int> entities = GetEntities(data);
        List<FDTrigType> excludedTypes = GetExcludedTypes();

        foreach (TR1Room room in level.Rooms)
        {
            RemoveSectorTriggers(level.FloorData, room.Sectors, entities, excludedTypes);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        List<int> entities = GetEntities(data);
        List<FDTrigType> excludedTypes = GetExcludedTypes();

        foreach (TR2Room room in level.Rooms)
        {
            RemoveSectorTriggers(level.FloorData, room.Sectors, entities, excludedTypes);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        List<int> entities = GetEntities(data);
        List<FDTrigType> excludedTypes = GetExcludedTypes();

        foreach (TR3Room room in level.Rooms)
        {
            RemoveSectorTriggers(level.FloorData, room.Sectors, entities, excludedTypes);
        }
    }

    private List<int> GetEntities(EMLevelData data)
    {
        List<int> entities = new();
        if (Entities != null)
        {
            entities.AddRange(Entities.Select(e => (int)data.ConvertEntity(e)));
        }
        return entities;
    }

    private List<FDTrigType> GetExcludedTypes()
    {
        List<FDTrigType> types = new();
        if (ExcludedTypes != null)
        {
            types.AddRange(ExcludedTypes);
        }
        return types;
    }

    private static void RemoveSectorTriggers(FDControl floorData, IEnumerable<TRRoomSector> sectors, List<int> entities, List<FDTrigType> excludedTypes)
    {
        foreach (TRRoomSector sector in sectors)
        {
            if (sector.FDIndex == 0)
            {
                continue;
            }

            List<FDEntry> entries = floorData[sector.FDIndex];
            // Find any trigger that isn't a type we wish to exclude
            if (entries.Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && !excludedTypes.Contains(trigger.TrigType))
            {
                trigger.Actions.RemoveAll(a => a.Action == FDTrigAction.Object && entities.Contains(a.Parameter));
                if (trigger.Actions.Count == 0)
                {
                    entries.Remove(trigger);
                }
            }
        }
    }
}
