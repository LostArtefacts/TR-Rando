using Newtonsoft.Json;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretAllocator : ISecretRandomizer
{
    private static readonly int _levelSecretCount = 3;

    private static readonly List<string> _textureFixLevels = new()
    {
        TR2LevelNames.FLOATER,
        TR2LevelNames.LAIR,
        TR2LevelNames.HOME
    };

    private readonly Dictionary<string, List<Location>> _locations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR2Entity> _secretPicker;

    public RandomizerSettings Settings { get; set; }
    public Random Generator { get; set; }
    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public TR2SecretAllocator()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR2\Locations\locations.json"));
        _routePicker = new(@"Resources\TR2\Locations\routes.json");
    }

    public IEnumerable<string> GetPacks()
    {
        return _locations.Values
            .SelectMany(v => v.Select(l => l.PackID))
            .Where(a => a != Location.DefaultPackID)
            .Distinct();
    }

    public void PlaceAllSecrets(string levelName, TR2Level level)
    {
        Queue<int> existingIndices = new();
        for (int i = 0; i < level.Entities.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType(level.Entities[i].TypeID))
            {
                existingIndices.Enqueue(i);
            }
        }

        _routePicker.Initialise(levelName, _locations[levelName], Settings, Generator);

        // Simulate zoning of sorts by splitting the route equally. This of course doesn't guarantee
        // what will be assigned in regular mode.
        List<short> routeRooms = _routePicker.GetRouteRooms();
        List<List<short>> zones = routeRooms.Split(_levelSecretCount);
        List<TR2Type> secretTypes = TR2TypeUtilities.GetSecretTypes();

        foreach (Location location in _locations[levelName])
        {
            TR2Entity entity;
            if (existingIndices.Count > 0)
            {
                entity = level.Entities[existingIndices.Dequeue()];
            }
            else
            {
                level.Entities.Add(entity = new());
            }

            int zone = zones.FindIndex(z => z.Contains(location.Room));
            PlaceSecret(entity, secretTypes[zone], location);
        }

        FixSecretTextures(levelName, level);
    }

    public List<Location> RandomizeSecrets(string levelName, TR2Level level)
    {
        if (!_locations.ContainsKey(levelName))
        {
            return new();
        }

        _secretPicker ??= new()
        {
            Settings = Settings,
            Generator = Generator,
            ItemFactory = ItemFactory,
            Mirrorer = Mirrorer,
            RouteManager = _routePicker,
        };

        List<Location> locations = _locations[levelName];
        locations.Shuffle(Generator);

        _secretPicker.SectorAction = loc => level.GetRoomSector(loc);
        _routePicker.RoomInfos = level.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _routePicker.Initialise(levelName, locations, Settings, Generator);

        // Organise picked locations according to route position to allow classic Stone/Jade/Gold order.
        List<Location> pickedLocations = _secretPicker.GetLocations(locations, Mirrorer?.IsMirrored(levelName) ?? false, _levelSecretCount);
        List<TR2Type> secretTypes = TR2TypeUtilities.GetSecretTypes();
        pickedLocations.Sort((l1, l2) =>
            _routePicker.GetRoutePosition(l1).CompareTo(_routePicker.GetRoutePosition(l2)));

        for (int i = 0; i < pickedLocations.Count; i++)
        {
            TR2Entity entity = level.Entities.Find(e => e.TypeID == secretTypes[i]);
            entity ??= ItemFactory.CreateItem(levelName, level.Entities);
            PlaceSecret(entity, secretTypes[i], pickedLocations[i]);
        }

        _secretPicker.FinaliseSecretPool(pickedLocations, levelName, itemIndex => GetDependentLockedItems(level, itemIndex));

        FixSecretTextures(levelName, level);

        return pickedLocations;
    }

    private static List<int> GetDependentLockedItems(TR2Level level, int itemIndex)
    {
        // We may be locking an enemy, so be sure to also lock their pickups.
        List<int> items = new() { itemIndex };

        if (TR2TypeUtilities.IsEnemyType(level.Entities[itemIndex].TypeID))
        {
            Location enemyLocation = level.Entities[itemIndex].GetLocation();
            List<TR2Entity> pickups = level.Entities
                .FindAll(e => TR2TypeUtilities.IsAnyPickupType(e.TypeID))
                .FindAll(e => e.GetLocation().IsEquivalent(enemyLocation));

            items.AddRange(pickups.Select(p => level.Entities.IndexOf(p)));
        }

        return items;
    }

    private void PlaceSecret(TR2Entity entity, TR2Type type, Location location)
    {
        _routePicker.SetLocation(entity, location);
        entity.TypeID = type;
        entity.Intensity1 = -1;
        entity.Intensity2 = -1;
        entity.Flags = 0;
    }

    private static void FixSecretTextures(string levelName, TR2Level level)
    {
        if (!_textureFixLevels.Contains(levelName))
        {
            return;
        }

        // Swap Stone and Jade textures - OG has them the wrong way around.
        (level.Sprites[TR2Type.JadeSecret_S_P].Textures, level.Sprites[TR2Type.StoneSecret_S_P].Textures)
            = (level.Sprites[TR2Type.StoneSecret_S_P].Textures, level.Sprites[TR2Type.JadeSecret_S_P].Textures);
    }
}
