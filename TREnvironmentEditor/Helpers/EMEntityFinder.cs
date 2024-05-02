using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Helpers;

public class EMEntityFinder
{
    public EMLocation Location { get; set; }
    public EMEntityType Type { get; set; }
    public List<short> Types { get; set; }

    public int GetEntity(TR1Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);

        List<TR1Type> types = new();
        if (Type == EMEntityType.Item)
        {
            types.AddRange(TR1TypeUtilities.GetStandardPickupTypes());
        }
        else if (Type == EMEntityType.KeyItem)
        {
            types.AddRange(TR1TypeUtilities.GetKeyItemTypes());
        }
        if (Types != null)
        {
            types.AddRange(Types.Select(t => (TR1Type)t));
        }

        return GetEntity(level.Entities, types, data, 
            l => level.GetRoomSector(l.X, l.Y, l.Z, l.Room));
    }

    public int GetEntity(TR2Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);

        List<TR2Type> types = new();
        if (Type == EMEntityType.Item)
        {
            types.AddRange(TR2TypeUtilities.GetStandardPickupTypes());
        }
        else if (Type == EMEntityType.KeyItem)
        {
            types.AddRange(TR2TypeUtilities.GetKeyItemTypes());
        }
        if (Types != null)
        {
            types.AddRange(Types.Select(t => (TR2Type)t));
        }

        return GetEntity(level.Entities, types, data,
            l => level.GetRoomSector(l.X, l.Y, l.Z, l.Room));
    }

    public int GetEntity(TR3Level level)
    {
        EMLevelData data = EMLevelData.GetData(level);

        List<TR3Type> types = new();
        if (Type == EMEntityType.Item)
        {
            types.AddRange(TR3TypeUtilities.GetStandardPickupTypes());
        }
        else if (Type == EMEntityType.KeyItem)
        {
            types.AddRange(TR3TypeUtilities.GetKeyItemTypes());
        }
        if (Types != null)
        {
            types.AddRange(Types.Select(t => (TR3Type)t));
        }

        return GetEntity(level.Entities, types, data,
            l => level.GetRoomSector(l.X, l.Y, l.Z, l.Room));
    }

    public int GetEntity<E, T>(List<E> entities, List<T> types, EMLevelData data, Func<EMLocation, TRRoomSector> sectorFunc)
        where E : TREntity<T>
        where T : Enum
    {
        EMLocation location = new()
        {
            X = Location.X,
            Y = Location.Y,
            Z = Location.Z,
            Room = data.ConvertRoom(Location.Room)
        };
        
        TRRoomSector sector = sectorFunc(location);
        for (int i = 0; i < entities.Count; i++)
        {
            E entity = entities[i];
            if (types.Contains(entity.TypeID)
                && entity.Room == location.Room
                && sectorFunc(new() { X = entity.X, Y = entity.Y, Z = entity.Z, Room = entity.Room }) == sector)
            {
                return i;
            }
        }

        return -1;
    }
}
