using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMClickFunction : BaseEMFunction
{
    // This differs from dedicated floor/ceiling functions by only shifting sector values and does not deal with faces.
    // See example in Masonic room in Aldwych.
    public EMLocation Location { get; set; }
    public List<EMLocation> Locations { get; set; }
    public EMLocationExpander LocationExpander { get; set; }
    public sbyte? FloorClicks { get; set; }
    public sbyte? CeilingClicks { get; set; }

    protected List<EMLocation> _locations;

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        _locations = InitialiseLocations(data);

        FDControl floorData = new FDControl();
        floorData.ParseFromLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
            MoveSector(sector, level.Rooms[location.Room].Info);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            if (FloorClicks.HasValue)
            {
                foreach (TREntity entity in level.Entities)
                {
                    if (entity.Room == location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        _locations = InitialiseLocations(data);

        FDControl floorData = new FDControl();
        floorData.ParseFromLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
            MoveSector(sector, level.Rooms[location.Room].Info);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            if (FloorClicks.HasValue)
            {
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        _locations = InitialiseLocations(data);

        FDControl floorData = new FDControl();
        floorData.ParseFromLevel(level);

        foreach (EMLocation location in _locations)
        {
            TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
            MoveSector(sector, level.Rooms[location.Room].Info);

            // Move any entities that share the same floor sector up or down the relevant number of clicks
            if (FloorClicks.HasValue)
            {
                foreach (TR2Entity entity in level.Entities)
                {
                    if (entity.Room == location.Room)
                    {
                        TRRoomSector entitySector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, level, floorData);
                        if (entitySector == sector)
                        {
                            entity.Y += GetEntityYShift(FloorClicks.Value);
                        }
                    }
                }
            }
        }
    }

    protected List<EMLocation> InitialiseLocations(EMLevelData data)
    {
        List<EMLocation> locations = new List<EMLocation>();
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

        foreach (EMLocation location in locations)
        {
            location.Room = data.ConvertRoom(location.Room);
        }

        // Remove any potential duplicates
        for (int i = locations.Count - 1; i >= 0; i--)
        {
            EMLocation loc1 = locations[i];
            locations.RemoveAll(loc2 => locations.IndexOf(loc2) != i && loc1.X == loc2.X && loc1.Y == loc2.Y && loc1.Z == loc2.Z && loc1.Room == loc2.Room);
        }

        return locations;
    }

    private void MoveSector(TRRoomSector sector, TRRoomInfo roomInfo)
    {
        if (sector.IsImpenetrable)
        {
            sector.Ceiling = (sbyte)(roomInfo.YTop / 256);
            sector.Floor = (sbyte)(roomInfo.YBottom / 256);
        }
        
        if (FloorClicks.HasValue)
        {
            sector.Floor += FloorClicks.Value;
        }
        if (CeilingClicks.HasValue)
        {
            sector.Ceiling += CeilingClicks.Value;
        }
    }

    protected virtual int GetEntityYShift(int clicks)
    {
        return clicks * 256;
    }
}
