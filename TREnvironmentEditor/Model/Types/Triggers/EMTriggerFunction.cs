using TREnvironmentEditor.Helpers;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMTriggerFunction : BaseEMFunction
{
    public List<EMLocation> Locations { get; set; }
    public List<short> Rooms { get; set; }
    public EMLocationExpander ExpandedLocations { get; set; }
    public int? EntityLocation { get; set; }
    public EMTrigger Trigger { get; set; }
    public bool Replace { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
                CreateTrigger(sector, level.FloorData, triggerEntry);
            }
        }

        if (Rooms != null)
        {
            foreach (short room in Rooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    if (!sector.IsWall && sector.RoomBelow == TRConsts.NoRoom)
                    {
                        CreateTrigger(sector, level.FloorData, triggerEntry);
                    }
                }
            }
        }

        if (EntityLocation.HasValue)
        {
            TR1Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            TRRoomSector sector = level.FloorData.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level);
            CreateTrigger(sector, level.FloorData, triggerEntry);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
                CreateTrigger(sector, level.FloorData, triggerEntry);
            }
        }

        if (Rooms != null)
        {
            foreach (short room in Rooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    if (!sector.IsWall && sector.RoomBelow == TRConsts.NoRoom)
                    {
                        CreateTrigger(sector, level.FloorData, triggerEntry);
                    }
                }
            }
        }

        if (EntityLocation.HasValue)
        {
            TR2Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            TRRoomSector sector = level.FloorData.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level);
            CreateTrigger(sector, level.FloorData, triggerEntry);
        }

        // Handle any specifics that the trigger may rely on
        foreach (FDActionItem action in triggerEntry.Actions)
        {
            switch (action.Action)
            {
                case FDTrigAction.ClearBodies:
                    SetEnemyClearBodies(level.Entities);
                    break;
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        FDTriggerEntry triggerEntry = InitialiseTriggerEntry(data);

        if (Locations != null)
        {
            foreach (EMLocation location in Locations)
            {
                TRRoomSector sector = level.FloorData.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level);
                CreateTrigger(sector, level.FloorData, triggerEntry);
            }
        }

        if (Rooms != null)
        {
            foreach (short room in Rooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    if (!sector.IsWall && sector.RoomBelow == TRConsts.NoRoom)
                    {
                        CreateTrigger(sector, level.FloorData, triggerEntry);
                    }
                }
            }
        }

        if (EntityLocation.HasValue)
        {
            TR3Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
            TRRoomSector sector = level.FloorData.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level);
            CreateTrigger(sector, level.FloorData, triggerEntry);
        }

        // Handle any specifics that the trigger may rely on
        foreach (FDActionItem action in triggerEntry.Actions)
        {
            switch (action.Action)
            {
                case FDTrigAction.ClearBodies:
                    SetEnemyClearBodies(level.Entities);
                    break;
            }
        }
    }

    private FDTriggerEntry InitialiseTriggerEntry(EMLevelData data)
    {
        // Expand locations
        if (ExpandedLocations != null)
        {
            Locations = ExpandedLocations.Expand();
        }
        
        return Trigger.ToFDEntry(data);
    }

    private void CreateTrigger(TRRoomSector sector, FDControl control, FDTriggerEntry triggerEntry)
    {
        // If there is no floor data create the FD to begin with.
        if (sector.FDIndex == 0)
        {
            control.CreateFloorData(sector);
        }

        List<FDEntry> entries = control[sector.FDIndex];
        if (Replace)
        {
            entries.Clear();
        }
        if (entries.FindIndex(e => e is FDTriggerEntry) == -1)
        {
            entries.Add(triggerEntry);
        }
    }

    private static void SetEnemyClearBodies(IEnumerable<TR2Entity> levelEntities)
    {
        foreach (TR2Entity entity in levelEntities)
        {
            if (TR2TypeUtilities.IsEnemyType(entity.TypeID))
            {
                entity.ClearBody = true;
            }
        }
    }

    private static void SetEnemyClearBodies(IEnumerable<TR3Entity> levelEntities)
    {
        foreach (TR3Entity entity in levelEntities)
        {
            if (TR2TypeUtilities.IsEnemyType((TR2Type)entity.TypeID))
            {
                entity.ClearBody = true;
            }
        }
    }
}
