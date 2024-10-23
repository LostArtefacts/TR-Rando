using Newtonsoft.Json;
using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers;

public class TR1RSecretRandomizer : BaseTR1RRandomizer, ISecretRandomizer
{
    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR1Entity> _secretPicker;
    private SecretArtefactPlacer<TR1Type, TR1Entity> _placer;

    public TR1RDataCache DataCache { get; set; }
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public TR1RSecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource("TR1/Locations/locations.json"));
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource("TR1/Locations/unarmed_locations.json"));
        _routePicker = new(GetResourcePath("TR1/Locations/routes.json"));
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
            RouteManager = _routePicker,
        };

        _placer = new()
        {
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        SetMessage("Randomizing secrets - loading levels");

        List<SecretProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new(this));
        }

        List<TR1RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        SetMessage("Randomizing secrets - importing models");
        processors.ForEach(p => p.Start());
        processors.ForEach(p => p.Join());

        if (!SaveMonitor.IsCancelled && _processingException == null)
        {
            SetMessage("Randomizing secrets - placing items");
            processors.ForEach(p => p.ApplyRandomization());
        }

        _processingException?.Throw();
    }

    private void PlaceAllSecrets(TR1RCombinedLevel level, List<TR1Type> pickupTypes)
    {
        if (!_locations.ContainsKey(level.Name))
        {
            return;
        }
        List<Location> locations = _locations[level.Name];
        _placer.InitialiseLevel(level);

        TRSecretPlacement<TR1Type> secret = new();
        int pickupIndex = 0;
        ushort secretIndex = 0;
        const ushort countedSecrets = 5;
        foreach (Location location in locations)
        {
            if (location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (short)(secretIndex % countedSecrets);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            _placer.PlaceSecret(secret);

            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, locations);
    }

    private void RandomizeSecrets(TR1RCombinedLevel level, List<TR1Type> pickupTypes)
    {
        if (!_locations.ContainsKey(level.Name))
        {
            return;
        }

        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);
        _placer.InitialiseLevel(level);

        _secretPicker.SectorAction = loc => level.Data.GetRoomSector(loc);
        _secretPicker.PlacementTestAction = loc => _placer.TestSecretPlacement(loc);

        _routePicker.RoomInfos = new(level.Data.Rooms.Select(r => new ExtRoomInfo(r)));
        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        List<Location> pickedLocations = _secretPicker.GetLocations(locations, false, level.Script.NumSecrets);
                
        TRSecretPlacement<TR1Type> secret = new();
        int pickupIndex = 0;
        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            Location location = pickedLocations[i];
            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            _placer.PlaceSecret(secret);

            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath($"TR1/SecretMapping/{level.Name}-SecretMapping.json"));
        _placer.CreateRewardStacks(level.Data.Entities, secretMapping.RewardEntities, level.Data.FloorData);

        AddDamageControl(level, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name, itemIndex => new() { itemIndex });
    }

    private void AddDamageControl(TR1RCombinedLevel level, List<Location> locations)
    {
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name)
            && ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
        {
            List<Location> pool = _unarmedLocations[level.Name];
            Location location = pool[_generator.Next(0, pool.Count)];

            TR1Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
            medi.TypeID = TR1Type.LargeMed_S_P;
        }
    }

    internal class SecretProcessor : AbstractProcessorThread<TR1RSecretRandomizer>
    {
        private static readonly Dictionary<TR1Type, TR1Type> _secretModels = TR1TypeUtilities.GetSecretModels();
        private static readonly Dictionary<TR1Type, TR1Type> _modelReplacements = TR1TypeUtilities.GetSecretReplacements();

        private readonly Dictionary<TR1RCombinedLevel, TRSecretModelAllocation<TR1Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR1RSecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new();
        }

        internal void AddLevel(TR1RCombinedLevel level)
        {
            _importAllocations.Add(level, new());
        }

        protected override void StartImpl()
        {
            List<TR1Type> availableTypes = _secretModels.Keys.ToList();
            foreach (TR1RCombinedLevel level in _importAllocations.Keys)
            {
                if (level.IsAssault)
                {
                    continue;
                }

                TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                foreach (TR1Type puzzleType in _modelReplacements.Keys)
                {
                    if (!level.Data.Models.ContainsKey(puzzleType))
                    {
                        allocation.AvailablePickupModels.Add(puzzleType);
                    }
                }
                TR1Type modelType = _outer.Settings.UseRandomSecretModels
                    ? availableTypes[_outer._generator.Next(0, availableTypes.Count)]
                    : TR1TypeUtilities.GetBestLevelSecretModel(level.Name);
                allocation.ImportModels.Add(modelType);
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR1RCombinedLevel level in _importAllocations.Keys)
            {
                if (!level.IsAssault)
                {
                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                    TR1DataImporter importer = new(true)
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        TypesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath("TR1/Objects"),
                    };
                    importer.Import();

                    foreach (TR1Type secretModelType in allocation.ImportModels)
                    {
                        TR1Type secretPickupType = _secretModels[secretModelType];

                        TR1Type puzzleModelType = allocation.AvailablePickupModels.First();
                        TR1Type puzzlePickupType = _modelReplacements[puzzleModelType];

                        level.Data.Models.ChangeKey(secretModelType, puzzleModelType);
                        level.Data.Sprites.ChangeKey(secretPickupType, puzzlePickupType);

                        allocation.AvailablePickupModels.RemoveAt(0);
                        allocation.AssignedPickupModels.Add(puzzlePickupType);

                        _outer.DataCache.SetData(level.PDPData, level.MapData, secretModelType, puzzleModelType);
                    }
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }

        internal void ApplyRandomization()
        {
            foreach (TR1RCombinedLevel level in _importAllocations.Keys)
            {
                if (!level.IsAssault)
                {
                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];
                    if (_outer.Settings.DevelopmentMode)
                    {
                        _outer.PlaceAllSecrets(level, allocation.AssignedPickupModels);
                    }
                    else
                    {
                        _outer.RandomizeSecrets(level, allocation.AssignedPickupModels);
                    }

                    _outer.SaveLevel(level);
                }

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }
    }
}
