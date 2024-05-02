using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAppendTriggerActionFunction : BaseEMFunction
{
    public EMLocation Location { get; set; }
    public List<EMLocation> Locations { get; set; }
    public EMLocationExpander LocationExpander { get; set; }
    public int? EntityLocation { get; set; }
    public List<EMTriggerAction> Actions { get; set; }
    public List<FDTrigType> TargetTypes { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        List<FDActionItem> actions = InitialiseActionItems(data);
        List<EMLocation> locations = InitialiseLocations<TR1Type, TR1Entity>(level.Entities, data);

        foreach (EMLocation location in locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
            AppendActions(sector, level.FloorData, actions);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        List<FDActionItem> actions = InitialiseActionItems(data);
        List<EMLocation> locations = InitialiseLocations<TR2Type, TR2Entity>(level.Entities, data);

        foreach (EMLocation location in locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
            AppendActions(sector, level.FloorData, actions);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        List<FDActionItem> actions = InitialiseActionItems(data);
        List<EMLocation> locations = InitialiseLocations<TR3Type, TR3Entity>(level.Entities, data);

        foreach (EMLocation location in locations)
        {
            TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
            AppendActions(sector, level.FloorData, actions);
        }
    }

    private List<EMLocation> InitialiseLocations<T, E>(List<E> entities, EMLevelData data)
        where E : TREntity<T>
        where T : Enum
    {
        List<EMLocation> locations = new();
        if (Location != null)
        {
            locations.Add(Location);
        }
        if (Locations != null)
        {
            locations.AddRange(Locations);
        }
        if (LocationExpander != null)
        {
            locations.AddRange(LocationExpander.Expand());
        }
        if (EntityLocation.HasValue)
        {
            E entity = entities[data.ConvertEntity(EntityLocation.Value)];
            locations.Add(new()
            {
                X = entity.X,
                Y = entity.Y,
                Z = entity.Z,
                Room = entity.Room,
            });
        }

        return locations;
    }

    private List<FDActionItem> InitialiseActionItems(EMLevelData data)
    {
        List<FDActionItem> actions = new();
        foreach (EMTriggerAction action in Actions)
        {
            actions.Add(action.ToFDAction(data));
        }
        return actions;
    }

    private void AppendActions(TRRoomSector sector, FDControl floorData, List<FDActionItem> actions)
    {
        if (sector.FDIndex != 0 && floorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
            && (TargetTypes == null || TargetTypes.Contains(trigger.TrigType)))
        {
            foreach (FDActionItem item in actions)
            {
                if (!trigger.Actions.Any(a => a.Action == item.Action && a.Parameter == item.Parameter))
                {
                    trigger.Actions.Add(item);
                }
            }
        }
    }
}
