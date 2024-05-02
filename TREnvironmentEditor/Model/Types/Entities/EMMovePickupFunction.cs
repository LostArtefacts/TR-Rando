using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMMovePickupFunction : BaseEMFunction
{
    public List<short> Types { get; set; }
    public List<EMLocation> SectorLocations { get; set; }
    public EMLocation TargetLocation { get; set; }
    public bool MatchY { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        MovePickups(level.Entities, data, location =>
            level.GetRoomSector(data.ConvertLocation(location)));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        MovePickups(level.Entities, data, location =>
            level.GetRoomSector(data.ConvertLocation(location)));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        MovePickups(level.Entities, data, location =>
            level.GetRoomSector(data.ConvertLocation(location)));
    }

    private void MovePickups<T>(IEnumerable<TREntity<T>> entities, EMLevelData data, Func<EMLocation, TRRoomSector> sectorGetter)
        where T : Enum
    {
        // Store the sectors we are interested in
        Dictionary<TRRoomSector, EMLocation> sectors = new();
        foreach (EMLocation location in SectorLocations)
        {
            TRRoomSector sector = sectorGetter.Invoke(location);
            sectors[sector] = location;
        }

        // Scan for each entity type and if it's found, find its sector location. If it matches
        // any we are interested in, move the item to the new location. If we haven't defined a
        // manual target location, the one used to locate the sector will be used.
        IEnumerable<TREntity<T>> matchingEntities;
        if (Types == null || Types.Count == 0)
        {
            // We want to match anything and move it in this instance.
            matchingEntities = entities;
        }
        else
        {
            // Only look for the types we are interested in.
            matchingEntities = entities.Where(e => Types.Select(t => (T)(object)(int)t).Contains(e.TypeID));
        }

        foreach (TREntity<T> match in matchingEntities)
        {
            TRRoomSector matchSector = sectorGetter.Invoke(new EMLocation
            {
                X = match.X,
                Y = match.Y,
                Z = match.Z,
                Room = match.Room
            });

            // MatchY means the defined sector location's Y val should be compared with the entity's Y val, for
            // instances where an item may be in mid-air (i.e. underwater) and another may be on the floor below it.
            if (sectors.ContainsKey(matchSector) && (!MatchY || sectors[matchSector].Y == match.Y))
            {
                EMLocation location = TargetLocation ?? sectors[matchSector];
                match.X = location.X;
                match.Y = location.Y;
                match.Z = location.Z;
                match.Room = data.ConvertRoom(location.Room);
            }
        }
    }
}
