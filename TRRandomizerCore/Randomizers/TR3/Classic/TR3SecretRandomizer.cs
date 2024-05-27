using Newtonsoft.Json;
using TRDataControl;
using TRGE.Core;
using TRGE.Core.Item.Enums;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRandomizer : BaseTR3Randomizer, ISecretRandomizer
{
    private static readonly ushort _devModeSecretCount = 6;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR3Entity> _secretPicker;
    private SecretArtefactPlacer<TR3Type, TR3Entity> _placer;

    internal TR3TextureMonitorBroker TextureMonitor { get; set; }
    public ItemFactory<TR3Entity> ItemFactory { get; set; }
    public IMirrorControl Mirrorer { get; set; }

    public TR3SecretRandomizer()
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
            Mirrorer = Mirrorer,
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
            processors.Add(new SecretProcessor(this));
        }

        List<TR3CombinedLevel> levels = new(Levels.Count);
        foreach (TR3ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR3CombinedLevel level in levels)
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

    private TRSecretRoom<TR3Entity> MakePlaceholderRewardRoom(TR3CombinedLevel level)
    {
        return _placer.MakePlaceholderRewardRoom(TRGameVersion.TR3, level.Name, level.Script.NumSecrets, level.Data.Entities);
    }

    private void ActualiseRewardRoom(TR3CombinedLevel level, TRSecretRoom<TR3Entity> placeholder)
    {
        TR3SecretMapping secretMapping = TR3SecretMapping.Get(level);
        if (secretMapping == null)
        {
            return;
        }

        // Are any rooms enforced based on level specifics?
        TRSecretRoom<TR3Entity> rewardRoom = secretMapping.Rooms.Find(r => r.HasUsageCondition);
        if (rewardRoom == null || !rewardRoom.UsageCondition.GetResult(level.Data))
        {
            do
            {
                rewardRoom = secretMapping.Rooms[_generator.Next(0, secretMapping.Rooms.Count)];
            }
            while (rewardRoom == null || rewardRoom.HasUsageCondition);
        }

        rewardRoom.Room.ApplyToLevel(level.Data);
        short roomIndex = (short)(level.Data.Rooms.Count - 1);

        _placer.CreateRewardRoom(level.Name,
            placeholder,
            rewardRoom,
            level.Data.Entities,
            level.Data.Cameras,
            TR3Type.LookAtItem_H,
            secretMapping.RewardEntities,
            level.Data.FloorData,
            roomIndex,
            Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets,
            t => TR3TypeUtilities.IsTrapdoor(t)
        );
    }

    private void PlaceAllSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR3Entity> rewardRoom)
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
            if (Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.NotMirrored)
                continue;

            if (!Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (short)(secretIndex % countedSecrets); // Cycle through each secret number
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count]; // Cycle through the types

            // #238 Point this secret to a specific camera and look-at target if applicable.
            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (short)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (short)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);
            _placer.PlaceSecret(secret);

            // This will either make a new entity or repurpose an old one
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, pickupTypes, locations);
    }

    private void RandomizeSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR3Entity> rewardRoom)
    {
        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);
        _placer.InitialiseLevel(level);

        _secretPicker.SectorAction = loc => level.Data.GetRoomSector(loc);
        _secretPicker.PlacementTestAction = loc
            => _placer.TestSecretPlacement(loc);

        _routePicker.RoomInfos = new(level.Data.Rooms.Select(r => new ExtRoomInfo(r)));
        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        List<Location> pickedLocations = _secretPicker.GetLocations(locations, Mirrorer.IsMirrored(level.Name), level.Script.NumSecrets);

        int pickupIndex = 0;
        TRSecretPlacement<TR3Type> secret = new();
        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            Location location = pickedLocations[i];
            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (short)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (short)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(level.Script.NumSecrets, rewardRoom.DoorIndices);
            _placer.PlaceSecret(secret);

            // This will either make a new entity or repurpose an old one. Ensure it is locked
            // to prevent item rando from potentially treating it as a key item.
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, pickupTypes, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name, itemIndex => GetDependentLockedItems(level, itemIndex));
    }

    private static List<int> GetDependentLockedItems(TR3CombinedLevel level, int itemIndex)
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

    private void AddDamageControl(TR3CombinedLevel level, List<TR3Type> pickupTypes, List<Location> locations)
    {
        // If we have used a secret that requires damage, add a large medi to an unarmed level
        // weapon location.
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name))
        {
            if (ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
            {
                List<Location> pool = _unarmedLocations[level.Name];
                Location location = pool[_generator.Next(0, pool.Count)];

                TR3Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
                medi.TypeID = TR3Type.LargeMed_P;
            }
            else
            {
                level.Script.AddStartInventoryItem(TR3Items.LargeMedi);
            }
        }

        // If we have also used a secret that requires damage and is glitched, add something to the
        // top ring to allow medi dupes.
        if (locations.Any(l => l.RequiresDamage && l.RequiresGlitch))
        {
            // If we have a spare model slot, duplicate one of the artefacts into this so that
            // we can add a hint with the item name. Otherwise, just re-use a puzzle item.
            Dictionary<TR3Type, TR3Type> artefacts = TR3TypeUtilities.GetArtefactReplacements();

            TR3Type availablePickupType = default;
            TR3Type availableMenuType = default;
            foreach (TR3Type pickupType in artefacts.Keys)
            {
                TR3Type menuType = artefacts[pickupType];
                if (!level.Data.Models.ContainsKey(menuType))
                {
                    availablePickupType = pickupType;
                    availableMenuType = menuType;
                    break;
                }
            }

            if (availableMenuType != default)
            {
                // We have a free slot, so duplicate a model
                TR3Type baseArtefact = pickupTypes[_generator.Next(0, pickupTypes.Count)];
                TRModel artefactMenuModel = level.Data.Models[artefacts[baseArtefact]];
                level.Data.Models[availableMenuType] = artefactMenuModel.Clone();

                // Add a script name - pull from GamestringRando once translations completed
                SetPuzzleTypeName(level, availablePickupType, "Infinite Medi Packs");
            }
            else
            {
                // Otherwise, just use something already available (no change in name)
                availablePickupType = pickupTypes[_generator.Next(0, pickupTypes.Count)];
            }

            level.Script.AddStartInventoryItem(ItemUtilities.ConvertToScriptItem(availablePickupType));
        }
    }

    private static void SetPuzzleTypeName(TR3CombinedLevel level, TR3Type itemType, string name)
    {
        if (TR3TypeUtilities.IsKeyType(itemType))
        {
            level.Script.Keys[(int)(itemType - TR3Type.Key1_P)] = name;
        }
        else if (TR3TypeUtilities.IsPuzzleType(itemType))
        {
            level.Script.Puzzles[(int)(itemType - TR3Type.Puzzle1_P)] = name;
        }
        else if (TR3TypeUtilities.IsQuestType(itemType))
        {
            level.Script.Pickups[(int)(itemType - TR3Type.Quest1_P)] = name;
        }
    }

    internal class SecretProcessor : AbstractProcessorThread<TR3SecretRandomizer>
    {
        private static readonly Dictionary<TR3Type, TR3Type> _artefactPickups = TR3TypeUtilities.GetArtefactPickups();
        private static readonly Dictionary<TR3Type, TR3Type> _artefactReplacements = TR3TypeUtilities.GetArtefactReplacements();

        // Move this to Gamestring Rando once implemented
        private static readonly Dictionary<TR3Type, string> _pickupNames = new()
        {
            [TR3Type.Infada_P] = "Secret Infada Stone",
            [TR3Type.OraDagger_P] = "Secret Ora Dagger",
            [TR3Type.Element115_P] = "Secret Element 115",
            [TR3Type.EyeOfIsis_P] = "Secret Eye of Isis",
            [TR3Type.Quest1_P] = "Secret Serpent Stone",
            [TR3Type.Quest2_P] = "Secret Hand of Rathmore"
        };

        private readonly Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR3SecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new Dictionary<TR3CombinedLevel, TRSecretModelAllocation<TR3Type>>();
        }

        internal void AddLevel(TR3CombinedLevel level)
        {
            _importAllocations.Add(level, new TRSecretModelAllocation<TR3Type>());
        }

        protected override void StartImpl()
        {
            foreach (TR3CombinedLevel level in _importAllocations.Keys)
            {
                if (!level.HasSecrets && !_outer.Settings.DevelopmentMode)
                {
                    continue;
                }

                TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                // Work out which models are available to replace as secret pickups.
                // We exclude artefacts from import if this level already has an 
                // actual artefact model, and we exclude current puzzle/key items
                // from the available switching pool.
                if (level.Is(TR3LevelNames.CRASH))
                {
                    // Special case for Crash Site, which is the only level that uses Quest1 (the swamp map).
                    // We want to reallocate this as a key to allow us to reuse Quest1 on import. Amend the
                    // models to become Key3 and update the script to match.
                    level.Data.Models.ChangeKey(TR3Type.Quest1_P, TR3Type.Key3_P);
                    level.Data.Models.ChangeKey(TR3Type.Quest1_M_H, TR3Type.Key3_M_H);
                    level.Script.Keys[2] = level.Script.Pickups[0];
                    level.Script.SetStartInventoryItems(new()
                    {
                        [TR3Items.Key3] = 1
                    });
                }

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
            foreach (TR3CombinedLevel level in _importAllocations.Keys)
            {
                if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Get the artefacts into the level and refresh the model list
                    TextureMonitor<TR3Type> monitor = _outer.TextureMonitor.CreateMonitor(level.Name, allocation.ImportModels);
                    TR3DataImporter importer = new()
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        TypesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath(@"TR3\Objects"),
                        TextureMonitor = monitor
                    };
                    importer.Import();

                    // Redefine the artefacts as puzzle models otherwise the level ends on pickup
                    foreach (TR3Type artefactPickupType in allocation.ImportModels)
                    {
                        TR3Type artefactMenuType = _artefactPickups[artefactPickupType];

                        TR3Type puzzlePickupType = allocation.AvailablePickupModels.First();
                        TR3Type puzzleMenuType = _artefactReplacements[puzzlePickupType];

                        level.Data.Models.ChangeKey(artefactPickupType, puzzlePickupType);

                        // #277 Most levels (beyond India) have the artefacts as menu models so we need
                        // to duplicate the models instead of replacing them, otherwise the carried-over
                        // artefacts from previous levels are invisible.
                        level.Data.Models[puzzleMenuType] = level.Data.Models[artefactMenuType].Clone();

                        // Remove this puzzle type from the available pool
                        allocation.AvailablePickupModels.RemoveAt(0);

                        // Make the pickup type available to assign to items
                        allocation.AssignedPickupModels.Add(puzzlePickupType);

                        // Assign a name for the script
                        SetPuzzleTypeName(level, puzzlePickupType, _pickupNames[artefactPickupType]);

                        // Tell the texture monitor that these artefacts are puzzle items
                        monitor.TypeMap[artefactPickupType] = puzzlePickupType;
                        monitor.TypeMap[artefactMenuType] = puzzleMenuType;
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
            foreach (TR3CombinedLevel level in _importAllocations.Keys)
            {
                // Only create new secrets if applicable
                if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Reward rooms can be conditionally chosen based on level state after placing secrets,
                    // but we need to make a placholder for door indices and masks to create those secrets.
                    TRSecretRoom<TR3Entity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

                    // Pass the list of artefacts we can use as pickups along with the temporary reward
                    // room to the secret placers.
                    if (_outer.Settings.DevelopmentMode)
                    {
                        _outer.PlaceAllSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                    }
                    else
                    {
                        _outer.RandomizeSecrets(level, allocation.AssignedPickupModels, rewardRoom);
                    }

                    // Convert the placeholder reward room into an actual room now that secrets are positioned.
                    _outer.ActualiseRewardRoom(level, rewardRoom);
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
