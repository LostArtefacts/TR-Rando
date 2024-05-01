using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMReplaceTriggerActionParameterFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public List<int> EntityLocations { get; set; }
    public EMTriggerAction Action { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }
        if (EntityLocations != null)
        {
            foreach (int entityIndex in EntityLocations)
            {
                TR1Entity entity = level.Entities[data.ConvertEntity(entityIndex)];
                TRRoomSector baseSector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }
        if (EntityLocations != null)
        {
            foreach (int entityIndex in EntityLocations)
            {
                TR2Entity entity = level.Entities[data.ConvertEntity(entityIndex)];
                TRRoomSector baseSector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        FDActionItem action = InitialiseActionItem(data);

        FDControl control = new();
        control.ParseFromLevel(level);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector baseSector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }
        if (EntityLocations != null)
        {
            foreach (int entityIndex in EntityLocations)
            {
                TR3Entity entity = level.Entities[data.ConvertEntity(entityIndex)];
                TRRoomSector baseSector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, control);
                ReplaceActionParameter(baseSector, control, action);
            }
        }

        control.WriteToLevel(level);
    }

    private FDActionItem InitialiseActionItem(EMLevelData data)
    {
        return Action.ToFDAction(data);
    }

    private static void ReplaceActionParameter(TRRoomSector baseSector, FDControl control, FDActionItem actionItem)
    {
        if (baseSector.FDIndex == 0)
        {
            return;
        }

        List<FDActionItem> actions = FDUtilities.GetActionListItems(control, actionItem.TrigAction, baseSector.FDIndex);
        foreach (FDActionItem action in actions)
        {
            action.Parameter = actionItem.Parameter;
        }
    }
}
