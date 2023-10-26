using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Randomizers;

public class LocationPicker
{
    private static readonly int _rotation = -8192;

    private List<Location> _locations;
    private Random _generator;

    public void Initialise(List<Location> globalLocations, Random generator)
    {
        _locations = globalLocations;
        _generator = generator;
    }

    public List<Location> GetLocations()
    {
        return _locations;
    }

    public Location GetPickupLocation()
    {
        return _locations[_generator.Next(0, _locations.Count)];
    }

    public void SetLocation<T>(TREntity<T> entity, Location location)
        where T : Enum
    {
        entity.X = location.X;
        entity.Y = location.Y;
        entity.Z = location.Z;
        entity.Room = (short)location.Room;
        entity.Angle = location.Angle;

        // Anything other than -1 means a sloped sector and so the location generator
        // will have picked a suitable angle for it. For flat sectors, spin the entities
        // around randomly for variety.
        if (entity.Angle == -1)
        {
            entity.Angle = (short)(_generator.Next(0, 8) * _rotation);
        }
    }

    public static Location CreateExcludedLocation<T>(TREntity<T> entity, Func<Location, TRRoomSector> sectorFunc)
        where T : Enum
    {
        Location location = new()
        {
            X = entity.X,
            Y = entity.Y,
            Z = entity.Z,
            Room = entity.Room,
        };

        TRRoomSector sector = sectorFunc(location);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            location.Y = (sector.Floor + 1) * TRConsts.Step1;
            location.Room = sector.RoomBelow;
            sector = sectorFunc(location);
        }

        location.Y = sector.Floor * TRConsts.Step1;
        return location;
    }
}
