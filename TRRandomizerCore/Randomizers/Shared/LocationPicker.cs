using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class LocationPicker
{
    private static readonly int _rotation = -8192;

    private List<Location> _locations;
    private List<Location> _usedTriggerLocations;
    private Random _generator;

    public Func<Location, bool> TriggerTestAction { get; set; }
    public Func<Location, bool> KeyItemTestAction { get; set; }

    public void Initialise(List<Location> globalLocations, Random generator)
    {
        _locations = globalLocations;
        _usedTriggerLocations = new();
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

    public Location GetKeyItemLocation<T>(List<Location> locations, TREntity<T> entity, bool hasPickupTrigger)
        where T : Enum
    {
        // Currently we pass in locations - once all key items are zoned,
        // we can simply use the auto-generated locations.

        if (!hasPickupTrigger)
        {
            return locations[_generator.Next(0, locations.Count)];
        }

        Location currentLocation = entity.GetLocation();

        // If there is a trigger for this key item that will be shifted by environment
        // changes, make sure to select a location that doesn't already have a trigger.
        Location location;
        do
        {
            location = locations[_generator.Next(0, locations.Count)];
            if (location.IsEquivalent(currentLocation))
            {
                // No need to test if we've selected the same tile
                break;
            }
        }
        while (TriggerTestAction(location) || _usedTriggerLocations.Contains(location) 
            || (KeyItemTestAction != null && !KeyItemTestAction(location)));

        _usedTriggerLocations.Add(location);
        return location;
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
}
