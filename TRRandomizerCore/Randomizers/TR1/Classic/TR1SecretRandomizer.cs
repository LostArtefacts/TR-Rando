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
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1SecretRandomizer : BaseTR1Randomizer, ISecretRandomizer
{
    private static readonly ushort _maxSecretCount = 5;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR1Entity> _secretPicker;
    private SecretArtefactPlacer<TR1Type, TR1Entity> _placer;

    public ItemFactory<TR1Entity> ItemFactory { get; set; }
    public IMirrorControl Mirrorer { get; set; }

    public TR1SecretRandomizer()
    {
        _locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\locations.json"));
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\unarmed_locations.json"));
        _routePicker = new(GetResourcePath(@"TR1\Locations\routes.json"));
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

        if (!Settings.UseSecretPack)
        {
            SetSecretCounts();
        }

        SetMessage("Randomizing secrets - loading levels");

        List<SecretProcessor> processors = new();
        for (int i = 0; i < _maxThreads; i++)
        {
            processors.Add(new SecretProcessor(this));
        }

        List<TR1CombinedLevel> levels = new(Levels.Count);
        foreach (TR1ScriptedLevel lvl in Levels)
        {
            levels.Add(LoadCombinedLevel(lvl));
            if (!TriggerProgress())
            {
                return;
            }
        }

        int processorIndex = 0;
        foreach (TR1CombinedLevel level in levels)
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

        TR1Script script = ScriptEditor.Script as TR1Script;
        script.FixPyramidSecretTrigger = false;

        if (Settings.UseRecommendedCommunitySettings)
        {
            script.Enable3dPickups = false;
            if (Settings.HardSecrets || Settings.GlitchedSecrets)
            {
                script.EnableBuffering = true;
            }
        }

        if (Settings.GlitchedSecrets)
        {
            script.FixDescendingGlitch = false;
            script.FixQwopGlitch = false;
            script.FixWallJumpGlitch = false;
            script.WalkToItems = false;
        }

        ScriptEditor.SaveScript();
    }

    private bool Are3DPickupsEnabled()
    {
        return !Settings.UseRecommendedCommunitySettings
            && (ScriptEditor.Script as TR1Script).Enable3dPickups;
    }

    private void SetSecretCounts()
    {
        List<TR1ScriptedLevel> levels = Levels.FindAll(l => !l.Is(TR1LevelNames.ASSAULT));

        switch (Settings.SecretCountMode)
        {
            case TRSecretCountMode.Shuffled:
                List<ushort> defaultCounts = levels.Select(l => l.NumSecrets).ToList();
                foreach (TR1ScriptedLevel level in levels)
                {
                    int countIndex = _generator.Next(0, defaultCounts.Count);
                    level.NumSecrets = defaultCounts[countIndex];
                    defaultCounts.RemoveAt(countIndex);
                }
                break;

            case TRSecretCountMode.Customized:
                int min = (int)Math.Max(1, Settings.MinSecretCount);
                int max = (int)Math.Min(_maxSecretCount, Settings.MaxSecretCount) + 1;
                foreach (TR1ScriptedLevel level in levels)
                {
                    level.NumSecrets = (ushort)_generator.Next(min, max);
                }
                break;

            default:
                break;
        }
    }

    private TRSecretRoom<TR1Entity> MakePlaceholderRewardRoom(TR1CombinedLevel level)
    {
        return _placer.MakePlaceholderRewardRoom(TRGameVersion.TR1, level.Name, level.Script.NumSecrets, level.Data.Entities);
    }

    private void ActualiseRewardRoom(TR1CombinedLevel level, TRSecretRoom<TR1Entity> placeholder)
    {
        TRSecretMapping<TR1Entity> secretMapping = TRSecretMapping<TR1Entity>.Get(GetResourcePath($@"TR1\SecretMapping\{level.Name}-SecretMapping.json"));
        if (secretMapping == null)
        {
            return;
        }

        if (placeholder == null)
        {
            _placer.CreateRewardStacks(level.Data.Entities, secretMapping.RewardEntities, level.Data.FloorData);
            return;
        }
        else if (secretMapping.Rooms.Count == 0)
        {
            return;
        }

        // Are any rooms enforced based on level specifics?
        TRSecretRoom<TR1Entity> rewardRoom = secretMapping.Rooms.Find(r => r.HasUsageCondition);
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
            TR1Type.CameraTarget_N,
            secretMapping.RewardEntities,
            level.Data.FloorData,
            roomIndex,
            Settings.DevelopmentMode ? _maxSecretCount : level.Script.NumSecrets,
            t => TR1TypeUtilities.IsTrapdoor(t)
        );
    }

    private void PlaceAllSecrets(TR1CombinedLevel level, List<TR1Type> pickupTypes, TRSecretRoom<TR1Entity> rewardRoom)
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
        ushort countedSecrets = _maxSecretCount;
        foreach (Location location in locations)
        {
            if (Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.NotMirrored)
                continue;

            if (!Mirrorer.IsMirrored(level.Name) && location.LevelState == LevelState.Mirrored)
                continue;

            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities, true);
            secret.SecretIndex = (short)(secretIndex % countedSecrets);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
            {
                secret.CameraIndex = (short)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                secret.CameraTarget = (short)rewardRoom.DoorIndices[0];
            }

            secret.SetMaskAndDoor(countedSecrets, rewardRoom.DoorIndices);
            _placer.PlaceSecret(secret);

            // This will either make a new entity or repurpose an old one
            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, locations);
    }

    private void RandomizeSecrets(TR1CombinedLevel level, List<TR1Type> pickupTypes, TRSecretRoom<TR1Entity> rewardRoom)
    {
        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);
        _placer.InitialiseLevel(level);

        _secretPicker.SectorAction = loc => level.Data.GetRoomSector(loc);
        _secretPicker.PlacementTestAction = loc => _placer.TestSecretPlacement(loc);

        _routePicker.RoomInfos = new(level.Data.Rooms.Select(r => new ExtRoomInfo(r)));
        _routePicker.Initialise(level.Name, locations, Settings, _generator);

        List<Location> pickedLocations = _secretPicker.GetLocations(locations, Mirrorer.IsMirrored(level.Name), level.Script.NumSecrets);

        int pickupIndex = 0;
        TRSecretPlacement<TR1Type> secret = new();
        for (int i = 0; i < level.Script.NumSecrets; i++)
        {
            Location location = pickedLocations[i];
            secret.Location = location;
            secret.EntityIndex = (short)ItemFactory.GetNextIndex(level.Name, level.Data.Entities);
            secret.PickupType = pickupTypes[pickupIndex % pickupTypes.Count];

            if (rewardRoom != null)
            {
                if (Settings.UseRewardRoomCameras && rewardRoom.HasCameras)
                {
                    secret.CameraIndex = (short)rewardRoom.CameraIndices[pickupIndex % rewardRoom.CameraIndices.Count];
                    secret.CameraTarget = (short)rewardRoom.DoorIndices[0];
                }

                secret.SetMaskAndDoor(level.Script.NumSecrets, rewardRoom.DoorIndices);
            }

            _placer.PlaceSecret(secret);

            // Turn off walk-to-items in TR1X if we are placing on a slope above water.
            if (!level.Data.Rooms[secret.Location.Room].ContainsWater
                && secret.Location.IsSlipperySlope(level.Data))
            {
                (ScriptEditor as TR1ScriptEditor).WalkToItems = false;
            }

            // This will either make a new entity or repurpose an old one. Ensure it is locked
            // to prevent item rando from potentially treating it as a key item.
            TR1Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location);
            entity.TypeID = secret.PickupType;

            secret.SecretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, pickedLocations);
        _secretPicker.FinaliseSecretPool(pickedLocations, level.Name, itemIndex => new() { itemIndex });
    }

    private void AddDamageControl(TR1CombinedLevel level, List<Location> locations)
    {
        // If we have used a secret that requires damage, add a large medi to an unarmed level
        // weapon location.
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name))
        {
            if (ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
            {
                List<Location> pool = _unarmedLocations[level.Name];
                Location location = pool[_generator.Next(0, pool.Count)];

                TR1Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
                medi.TypeID = TR1Type.LargeMed_S_P;
            }
            else
            {
                level.Script.AddStartInventoryItem(TR1Items.LargeMed_S_P);
            }
        }

        // If we have also used a secret that requires damage and is glitched, add an additional
        // medi as these tend to occur where Lara has to drop far after picking them up.
        if (locations.Any(l => l.RequiresDamage && l.RequiresGlitch))
        {
            level.Script.AddStartInventoryItem(TR1Items.SmallMed_S_P);
        }
    }

    private static void SetPuzzleTypeName(TR1CombinedLevel level, TR1Type itemType, string name)
    {
        if (TR1TypeUtilities.IsKeyType(itemType))
        {
            PopulateScriptStrings((int)(itemType - TR1Type.Key1_S_P), level.Script.Keys, "K");
            level.Script.Keys.Add(name);
        }
        else if (TR1TypeUtilities.IsPuzzleType(itemType))
        {
            PopulateScriptStrings((int)(itemType - TR1Type.Puzzle1_S_P), level.Script.Puzzles, "P");
            level.Script.Puzzles.Add(name);
        }
        else if (TR1TypeUtilities.IsQuestType(itemType))
        {
            PopulateScriptStrings((int)(itemType - TR1Type.Quest1_S_P), level.Script.Pickups, "Q");
            level.Script.Pickups.Add(name);
        }
    }

    private static void PopulateScriptStrings(int index, List<string> gameStrings, string id)
    {
        while (index > gameStrings.Count)
        {
            gameStrings.Add(id + (gameStrings.Count + 1));
        }
    }

    internal class SecretProcessor : AbstractProcessorThread<TR1SecretRandomizer>
    {
        private static readonly Dictionary<TR1Type, TR1Type> _secretModels = TR1TypeUtilities.GetSecretModels();
        private static readonly Dictionary<TR1Type, TR1Type> _modelReplacements = TR1TypeUtilities.GetSecretReplacements();

        // Move this to Gamestring Rando once implemented
        private static readonly Dictionary<TR1Type, string> _pickupNames = new()
        {
            [TR1Type.SecretAnkh_M_H] = "Secret Ankh",
            [TR1Type.SecretGoldBar_M_H] = "Secret Gold Bar",
            [TR1Type.SecretGoldIdol_M_H] = "Secret Gold Idol",
            [TR1Type.SecretLeadBar_M_H] = "Secret Lead Bar",
            [TR1Type.SecretScion_M_H] = "Secret Scion"
        };

        private readonly Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TR1Type>> _importAllocations;

        internal override int LevelCount => _importAllocations.Count;

        internal SecretProcessor(TR1SecretRandomizer outer)
            : base(outer)
        {
            _importAllocations = new Dictionary<TR1CombinedLevel, TRSecretModelAllocation<TR1Type>>();
        }

        internal void AddLevel(TR1CombinedLevel level)
        {
            _importAllocations.Add(level, new TRSecretModelAllocation<TR1Type>());
        }

        protected override void StartImpl()
        {
            List<TR1Type> availableTypes = _secretModels.Keys.ToList();
            foreach (TR1CombinedLevel level in _importAllocations.Keys)
            {
                if (level.IsAssault)
                {
                    continue;
                }

                TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                // Work out which models are available to replace as secret pickups.
                // We exclude current puzzle/key items from the available switching pool.
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
            foreach (TR1CombinedLevel level in _importAllocations.Keys)
            {
                if (!level.IsAssault)
                {
                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                    // Get the artefacts into the level and refresh the model list
                    TR1DataImporter importer = new(true)
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        TypesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath(@"TR1\Objects"),
                    };

                    importer.Import();

                    // Redefine the artefacts as puzzle models
                    foreach (TR1Type secretModelType in allocation.ImportModels)
                    {
                        TR1Type secretPickupType = _secretModels[secretModelType];

                        TR1Type puzzleModelType = allocation.AvailablePickupModels.First();
                        TR1Type puzzlePickupType = _modelReplacements[puzzleModelType];

                        level.Data.Models.ChangeKey(secretModelType, puzzleModelType);
                        level.Data.Sprites.ChangeKey(secretPickupType, puzzlePickupType);

                        if (secretModelType == TR1Type.SecretScion_M_H && _outer.Are3DPickupsEnabled())
                        {
                            // TR1X embeds scions into the ground when they are puzzle/key types in 3D mode,
                            // so we counteract that here to avoid uncollectable items.
                            TRMesh scionMesh = level.Data.Models[puzzleModelType].Meshes[0];
                            foreach (TRVertex vertex in scionMesh.Vertices)
                            {
                                vertex.Y -= 90;
                            }
                        }

                        // Remove this puzzle type from the available pool
                        allocation.AvailablePickupModels.RemoveAt(0);

                        // Make the pickup type available to assign to items
                        allocation.AssignedPickupModels.Add(puzzlePickupType);

                        // Assign a name for the script
                        SetPuzzleTypeName(level, puzzlePickupType, _pickupNames[secretModelType]);
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
            foreach (TR1CombinedLevel level in _importAllocations.Keys)
            {
                if (!level.IsAssault)
                {
                    TRSecretModelAllocation<TR1Type> allocation = _importAllocations[level];

                    // Reward rooms can be conditionally chosen based on level state after placing secrets,
                    // but we need to make a placholder for door indices and masks to create those secrets.
                    TRSecretRoom<TR1Entity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

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
