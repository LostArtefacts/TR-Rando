using Newtonsoft.Json;
using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3RSecretRandomizer : BaseTR3RRandomizer, ISecretRandomizer
{
    private static readonly ushort _devModeSecretCount = 6;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR3Entity> _secretPicker;
    private SecretArtefactPlacer<TR3Type, TR3Entity> _placer;

    public TR3RDataCache DataCache { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public TR3RSecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\locations.json"));
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\unarmed_locations.json"));
        _routePicker = new(GetResourcePath(@"TR3\Locations\routes.json"));
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

        List<TR3RCombinedLevel> levels = new(Levels.Count);
        foreach (TRRScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR3RCombinedLevel level in levels)
        {
            processors[processorIndex].AddLevel(level);
            processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
        }

        SetMessage("Randomizing secrets - importing models");
        foreach (SecretProcessor processor in processors)
        {
            processor.Start();
        }

        foreach (SecretProcessor processor in processors)
        {
            processor.Join();
        }

        if (!SaveMonitor.IsCancelled && _processingException == null)
        {
            SetMessage("Randomizing secrets - placing items");
            foreach (SecretProcessor processor in processors)
            {
                processor.ApplyRandomization();
            }
        }

        _processingException?.Throw();
    }

    private void PlaceAllSecrets(TR3RCombinedLevel level, List<TR3Type> pickupTypes)
    {
        if (!_locations.ContainsKey(level.Name))
        {
            return;
        }
        List<Location> locations = _locations[level.Name];
        _placer.InitialiseLevel(level);

        TRSecretPlacement<TR3Type> secret = new();
        int pickupIndex = 0;
        ushort secretIndex = 0;
        ushort countedSecrets = _devModeSecretCount; // For dev mode test the max number of secrets in TR3
        foreach (Location location in locations)
        {
            if (location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (short)(secretIndex % countedSecrets); // Cycle through each secret number
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

            
            _placer.PlaceSecret(secret);

            // This will either make a new entity or repurpose an old one
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, locations);
    }

    private void RandomizeSecrets(TR3RCombinedLevel level, List<TR3Type> pickupTypes)
    {
        _placer.InitialiseLevel(level);
        if (level.Script.NumSecrets == 0)
        {
            return;
        }

        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);

        _secretPicker.SectorAction = loc => level.Data.GetRoomSector(loc);
        _secretPicker.PlacementTestAction = loc
            => _placer.TestSecretPlacement(loc);

        _routePicker.RoomInfos = new(level.Data.Rooms.Select(r => new ExtRoomInfo(r)));
        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        List<Location> pickedLocations = _secretPicker.GetLocations(locations, false, level.Script.NumSecrets);

        // We can't make reward rooms, so the items are instead distrbuted around the secret locations
        TRSecretMapping<TR3Entity> secretMapping = TRSecretMapping<TR3Entity>.Get(GetResourcePath($@"TR3\SecretMapping\{level.Name}-SecretMapping.json"));
        List<TR3Entity>[] rewardClusters = secretMapping.RewardEntities
            .Select(i => level.Data.Entities[i])
            .Cluster(level.Script.NumSecrets);

        int pickupIndex = 0;
        TRSecretPlacement<TR3Type> secret = new();
        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            Location location = pickedLocations[i];
            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            _placer.PlaceSecret(secret);
            rewardClusters[i].ForEach(r => r.SetLocation(location));

            // This will either make a new entity or repurpose an old one. Ensure it is locked
            // to prevent item rando from potentially treating it as a key item.
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name, itemIndex => GetDependentLockedItems(level, itemIndex));
    }

    private static List<int> GetDependentLockedItems(TR3RCombinedLevel level, int itemIndex)
    {
        // We may be locking an enemy, so be sure to also lock their pickups.
        List<int> items = new() { itemIndex };

        if (TR3TypeUtilities.IsEnemyType(level.Data.Entities[itemIndex].TypeID))
        {
            Location enemyLocation = level.Data.Entities[itemIndex].GetLocation();
            List<TR3Entity> pickups = level.Data.Entities
                .FindAll(e => TR3TypeUtilities.IsAnyPickupType(e.TypeID))
                .FindAll(e => e.GetLocation().IsEquivalent(enemyLocation));

            items.AddRange(pickups.Select(p => level.Data.Entities.IndexOf(p)));
        }

        return items;
    }

    private void AddDamageControl(TR3RCombinedLevel level, List<Location> locations)
    {
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name)
            && ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
        {
            List<Location> pool = _unarmedLocations[level.Name];
            Location location = pool[_generator.Next(0, pool.Count)];

            TR3Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
            medi.TypeID = TR3Type.LargeMed_P;
        }
    }

    internal class SecretProcessor : AbstractProcessorThread<TR3RSecretRandomizer>
    {
        private static readonly Dictionary<TR3Type, TR3Type> _artefactPickups = TR3TypeUtilities.GetArtefactPickups();
        private static readonly Dictionary<TR3Type, TR3Type> _artefactReplacements = TR3TypeUtilities.GetArtefactReplacements();

        private readonly Dictionary<TR3RCombinedLevel, TRSecretModelAllocation<TR3Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR3RSecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new();
        }

        internal void AddLevel(TR3RCombinedLevel level)
        {
            _importAllocations.Add(level, new());
        }

        protected override void StartImpl()
        {
            foreach (TR3RCombinedLevel level in _importAllocations.Keys)
            {
                if (!level.Script.HasSecrets && !_outer.Settings.DevelopmentMode)
                {
                    continue;
                }

                TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];
                allocation.AvailablePickupModels.AddRange(_artefactReplacements.Keys
                    .Where(a => !level.Data.Models.ContainsKey(a)));

                List<TR3Type> artefactTypes = _artefactPickups.Keys.ToList();
                artefactTypes.RemoveAll(a => level.Data.Models.ContainsKey(a));

                // How many models do we actually need?
                int modelImportCount = Math.Min(_outer.Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets, allocation.AvailablePickupModels.Count);
                modelImportCount = Math.Min(modelImportCount, artefactTypes.Count);

                for (int i = 0; i < modelImportCount; i++)
                {
                    allocation.ImportModels.Add(artefactTypes[i]);
                }
            }
        }

        protected override void ProcessImpl()
        {
            foreach (TR3RCombinedLevel level in _importAllocations.Keys)
            {
                if (level.Script.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Get the artefacts into the level and refresh the model list
                    TR3DataImporter importer = new()
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        TypesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath(@"TR3\Objects"),
                    };
                    importer.Import();

                    // Redefine the artefacts as puzzle models otherwise the level ends on pickup
                    foreach (TR3Type artefactPickupType in allocation.ImportModels)
                    {
                        TR3Type artefactMenuType = _artefactPickups[artefactPickupType];

                        TR3Type puzzlePickupType = allocation.AvailablePickupModels.First();
                        TR3Type puzzleMenuType = _artefactReplacements[puzzlePickupType];

                        level.Data.Models.ChangeKey(artefactPickupType, puzzlePickupType);

                        level.Data.Models[puzzleMenuType] = level.Data.Models[artefactMenuType].Clone();

                        _outer.DataCache.SetData(level.PDPData, level.MapData, artefactPickupType, puzzlePickupType);
                        _outer.DataCache.SetData(level.PDPData, level.MapData, artefactMenuType, puzzleMenuType);

                        allocation.AvailablePickupModels.RemoveAt(0);
                        allocation.AssignedPickupModels.Add(puzzlePickupType);
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
            foreach (TR3RCombinedLevel level in _importAllocations.Keys)
            {
                // Only create new secrets if applicable
                if (level.Script.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Pass the list of artefacts we can use as pickups along with the temporary reward
                    // room to the secret placers.
                    if (_outer.Settings.DevelopmentMode)
                    {
                        _outer.PlaceAllSecrets(level, allocation.AssignedPickupModels);
                    }
                    else
                    {
                        _outer.RandomizeSecrets(level, allocation.AssignedPickupModels);
                    }
                }

                _outer.SaveLevel(level);

                if (!_outer.TriggerProgress())
                {
                    break;
                }
            }
        }
    }
}
