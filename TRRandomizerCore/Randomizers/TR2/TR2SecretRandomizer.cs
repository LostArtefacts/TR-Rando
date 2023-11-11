using Newtonsoft.Json;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2SecretRandomizer : BaseTR2Randomizer, ISecretRandomizer
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
    private SecretPicker _secretPicker;

    public IMirrorControl Mirrorer { get; set; }
    public ItemFactory ItemFactory { get; set; }

    public TR2SecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\locations.json"));
        _routePicker = new(GetResourcePath(@"TR2\Locations\routes.json"));
    }

    public IEnumerable<string> GetPacks()
    {
        return _locations.Values
            .SelectMany(v => v.Select(l => l.PackID))
            .Where(a => a != Location.DefaultPackID)
            .Distinct();
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _secretPicker = new()
        {
            Settings = Settings,
            Generator = _generator,
            ItemFactory = ItemFactory,
            Mirrorer = Mirrorer,
            RouteManager = _routePicker
        };

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            if (_locations.ContainsKey(_levelInstance.Name))
            {
                if (Settings.DevelopmentMode)
                {
                    PlaceAllSecrets(_levelInstance);
                }
                else
                {
                    RandomizeSecrets(_levelInstance);
                }
            }

            FixSecretTextures(_levelInstance);
            SaveLevelInstance();

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void PlaceAllSecrets(TR2CombinedLevel level)
    {
        Queue<int> existingIndices = new();
        for (int i = 0; i < level.Data.Entities.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType(level.Data.Entities[i].TypeID))
            {
                existingIndices.Enqueue(i);
            }
        }

        _routePicker.Initialise(level.Name, _locations[level.Name], Settings, _generator);

        // Simulate zoning of sorts by splitting the route equally. This of course doesn't guarantee
        // what will be assigned in regular mode.
        List<int> routeRooms = _routePicker.GetRouteRooms();
        List<List<int>> zones = routeRooms.Split(_levelSecretCount);
        List<TR2Type> secretTypes = TR2TypeUtilities.GetSecretTypes();

        foreach (Location location in _locations[level.Name])
        {
            TR2Entity entity;
            if (existingIndices.Count > 0)
            {
                entity = level.Data.Entities[existingIndices.Dequeue()];
            }
            else
            {
                level.Data.Entities.Add(entity = new());
            }

            int zone = zones.FindIndex(z => z.Contains(location.Room));
            PlaceSecret(entity, secretTypes[zone], location);
        }

        AddDamageControl(level, _locations[level.Name]);
    }

    private void RandomizeSecrets(TR2CombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);

        _secretPicker.SectorAction = loc 
            => FDUtilities.GetRoomSector(loc.X, loc.Y, loc.Z, (short)loc.Room, level.Data, floorData);
        _routePicker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        // Organise picked locations according to route position to allow classic Stone/Jade/Gold order.
        List<Location> pickedLocations = _secretPicker.GetLocations(locations, Mirrorer.IsMirrored(level.Name), _levelSecretCount);
        List<TR2Type> secretTypes = TR2TypeUtilities.GetSecretTypes();
        pickedLocations.Sort((l1, l2) => 
            _routePicker.GetRoutePosition(l1).CompareTo(_routePicker.GetRoutePosition(l2)));

        for (int i = 0; i < pickedLocations.Count; i++)
        {
            TR2Entity entity = level.Data.Entities.Find(e => e.TypeID == secretTypes[i]);
            entity ??= ItemFactory.CreateItem(level.Name, level.Data.Entities);
            PlaceSecret(entity, secretTypes[i], pickedLocations[i]);
        }

        AddDamageControl(level, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name);
    }

    private void PlaceSecret(TR2Entity entity, TR2Type type, Location location)
    {
        _routePicker.SetLocation(entity, location);
        entity.TypeID = type;
        entity.Intensity1 = -1;
        entity.Intensity2 = -1;
        entity.Flags = 0;
    }

    private static void FixSecretTextures(TR2CombinedLevel level)
    {
        if (!_textureFixLevels.Contains(level.Name))
        {
            return;
        }

        // Swap Stone and Jade textures - OG has them the wrong way around.
        // SpriteSequence offsets have to remain in order, so swap the texture targets instead.
        TRSpriteSequence stoneSequence = Array.Find(level.Data.SpriteSequences, s => s.SpriteID == (int)TR2Type.StoneSecret_S_P);
        TRSpriteSequence jadeSequence = Array.Find(level.Data.SpriteSequences, s => s.SpriteID == (int)TR2Type.JadeSecret_S_P);

        TRSpriteTexture[] textures = level.Data.SpriteTextures;
        (textures[jadeSequence.Offset], textures[stoneSequence.Offset])
            = (textures[stoneSequence.Offset], textures[jadeSequence.Offset]);
    }

    private static void AddDamageControl(TR2CombinedLevel level, List<Location> locations)
    {
        IEnumerable<Location> damagingLocations = locations.Where(l => l.RequiresDamage);
        if (!damagingLocations.Any())
        {
            return;
        }

        uint easyDamageCount = 0;
        uint hardDamageCount = 0;

        foreach (Location location in damagingLocations)
        {
            if (location.Difficulty == Difficulty.Hard)
            {
                hardDamageCount++;
            }
            else
            {
                easyDamageCount++;
            }
        }

        if (level.Script.RemovesWeapons)
        {
            hardDamageCount++;
        }

        if (level.Sequence < 2)
        {
            easyDamageCount++;
        }

        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.LargeMed_S_P), hardDamageCount);
        level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(TR2Type.SmallMed_S_P), easyDamageCount);
    }
}
