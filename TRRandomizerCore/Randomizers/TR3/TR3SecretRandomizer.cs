using Newtonsoft.Json;
using System.Diagnostics;
using TRGE.Core;
using TRGE.Core.Item.Enums;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Secrets;
using TRRandomizerCore.Textures;
using TRRandomizerCore.Utilities;
using SC = TRRandomizerCore.Randomizers.SecretConsts;

namespace TRRandomizerCore.Randomizers;

public class TR3SecretRandomizer : BaseTR3Randomizer, ISecretRandomizer
{
    private static readonly ushort _devModeSecretCount = 6;

    private readonly Dictionary<string, List<Location>> _locations, _unarmedLocations;
    private readonly LocationPicker _routePicker;
    private SecretPicker<TR3Entity> _secretPicker;

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

    private static void RemoveDefaultSecrets(TR3CombinedLevel level)
    {
        // Scan all rooms and remove any existing secret triggers.
        foreach (TR3Room room in level.Data.Rooms)
        {
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (sector.FDIndex == 0)
                {
                    continue;
                }

                List<FDEntry> entries = level.Data.FloorData[sector.FDIndex];
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (entries[i] is FDTriggerEntry trig)
                    {
                        // #230 Remove the secret action but retain anything else that may be triggered here
                        trig.Actions.RemoveAll(a => a.Action == FDTrigAction.SecretFound);
                        if (trig.Actions.Count == 0)
                        {
                            entries.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

    private TRSecretRoom<TR2Entity> MakePlaceholderRewardRoom(TR3CombinedLevel level)
    {
        TRSecretRoom<TR2Entity> rewardRoom = null;
        string mappingPath = @"TR3\SecretMapping\" + level.Name + "-SecretMapping.json";
        if (ResourceExists(mappingPath))
        {
            // Trigger activation masks have 5 bits so we need a specific number of doors to match.
            // For development mode, test the maximum.
            double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
            int requiredDoors = (int)Math.Ceiling(countedSecrets / TRConsts.MaskBits);

            // Make the doors and store the entity indices for the secret triggers
            rewardRoom = new TRSecretRoom<TR2Entity>
            {
                DoorIndices = new List<int>()
            };
            for (int i = 0; i < requiredDoors; i++)
            {
                TR3Entity door = ItemFactory.CreateItem(level.Name, level.Data.Entities);
                rewardRoom.DoorIndices.Add(level.Data.Entities.IndexOf(door));
            }
        }

        return rewardRoom;
    }

    private void ActualiseRewardRoom(TR3CombinedLevel level, TRSecretRoom<TR2Entity> placeholder)
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

        // Convert the temporary doors
        rewardRoom.DoorIndices = placeholder.DoorIndices;
        for (int i = 0; i < rewardRoom.DoorIndices.Count; i++)
        {
            int doorIndex = rewardRoom.DoorIndices[i];
            TR3Entity door = rewardRoom.Doors[i];
            if (door.Room < 0)
            {
                door.Room = roomIndex;
            }
            level.Data.Entities[doorIndex] = door;

            // If it's a trapdoor, we need to make a dummy trigger for it
            if (TR3TypeUtilities.IsTrapdoor(door.TypeID))
            {
                CreateTrapdoorTrigger(door, (short)doorIndex, level.Data);
            }
        }

        // Spread the rewards out fairly evenly across each defined position in the new room.
        int rewardPositionCount = rewardRoom.RewardPositions.Count;
        for (int i = 0; i < secretMapping.RewardEntities.Count; i++)
        {
            TR3Entity item = level.Data.Entities[secretMapping.RewardEntities[i]];
            Location position = rewardRoom.RewardPositions[i % rewardPositionCount];

            item.X = position.X;
            item.Y = position.Y;
            item.Z = position.Z;
            item.Room = roomIndex;
        }

        // #238 Make the required number of cameras. Because of the masks, we need
        // a camera per counted secret otherwise it only shows once.
        if (Settings.UseRewardRoomCameras && rewardRoom.Cameras != null)
        {
            double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : level.Script.NumSecrets;
            rewardRoom.CameraIndices = new List<int>();
            for (int i = 0; i < countedSecrets; i++)
            {
                rewardRoom.CameraIndices.Add(level.Data.Cameras.Count);
                level.Data.Cameras.Add(rewardRoom.Cameras[i % rewardRoom.Cameras.Count]);
            }

            // Get each trigger created for each secret index and add the camera, provided
            // there isn't any existing camera actions.
            for (int i = 0; i < countedSecrets; i++)
            {
                List<FDTriggerEntry> secretTriggers = level.Data.FloorData.GetSecretTriggers(i);
                foreach (FDTriggerEntry trigger in secretTriggers)
                {
                    if (trigger.Actions.Find(a => a.Action == FDTrigAction.Camera) == null)
                    {
                        trigger.Actions.Add(new FDActionItem
                        {
                            Action = FDTrigAction.Camera,
                            CamAction = new()
                            {
                                Timer = 4
                            },
                            Parameter = (short)rewardRoom.CameraIndices[i]
                        });
                        trigger.Actions.Add(new FDActionItem
                        {
                            Action = FDTrigAction.LookAtItem,
                            Parameter = (short)rewardRoom.DoorIndices[0]
                        });
                    }
                }
            }
        }
    }

    private static void CreateTrapdoorTrigger(TR3Entity door, short doorIndex, TR3Level level)
    {
        TRRoomSector sector = level.GetRoomSector(door);
        if (sector.FDIndex == 0)
        {
            level.FloorData.CreateFloorData(sector);
        }

        level.FloorData[sector.FDIndex].Add(new FDTriggerEntry
        {
            TrigType = FDTrigType.Dummy,
            Actions = new()
            {
                new()
                {
                    Parameter = doorIndex
                }
            }
        });
    }

    private void PlaceAllSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
    {
        List<Location> locations = _locations[level.Name];

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
            PlaceSecret(level, secret);

            // This will either make a new entity or repurpose an old one
            TR3Entity entity = ItemFactory.CreateLockedItem(level.Name, level.Data.Entities, secret.Location, true);
            entity.TypeID = secret.PickupType;

            secretIndex++;
            pickupIndex++;
        }

        AddDamageControl(level, pickupTypes, locations);
    }

    private void RandomizeSecrets(TR3CombinedLevel level, List<TR3Type> pickupTypes, TRSecretRoom<TR2Entity> rewardRoom)
    {
        List<Location> locations = _locations[level.Name];
        locations.Shuffle(_generator);

        _secretPicker.SectorAction = loc => level.Data.GetRoomSector(loc);
        _secretPicker.PlacementTestAction = loc
            => TestSecretPlacement(level, loc);

        _routePicker.RoomInfos = level.Data.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();
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
            PlaceSecret(level, secret);

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

    private bool TestSecretPlacement(TR3CombinedLevel level, Location location)
    {
        // Check if this secret is being added to a flipped room, as that won't work
        for (int i = 0; i < level.Data.Rooms.Count; i++)
        {
            if (level.Data.Rooms[i].AlternateRoom == location.Room)
            {
                if (Settings.DevelopmentMode)
                {
                    // Place it anyway in dev mode to allow relocating
                    Debug.WriteLine(string.Format(SC.FlipMapErrorMsg, level.Name, location.X, location.Y, location.Z, location.Room));
                    break;
                }
                else
                {
                    return false;
                }
            }
        }

        // Get the sector and check if it is shared with a trapdoor or bridge, as these won't work either.
        TRRoomSector sector = level.Data.GetRoomSector(location);
        foreach (TR3Entity otherEntity in level.Data.Entities)
        {
            TR3Type type = otherEntity.TypeID;
            if (location.Room == otherEntity.Room && (TR3TypeUtilities.IsTrapdoor(type) || TR3TypeUtilities.IsBridge(type)))
            {
                TRRoomSector otherSector = level.Data.GetRoomSector(otherEntity);
                if (otherSector == sector)
                {
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(SC.TrapdoorLocationMsg, level.Name, location.X, location.Y, location.Z, location.Room));
                    }
                    return false;
                }
            }
        }

        if (!TestTriggerPlacement(level, location, (short)location.Room, sector))
        {
            return false;
        }

        // If the room has a flipmap, make sure to test the trigger there too.
        short altRoom = level.Data.Rooms[location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            sector = level.Data.GetRoomSector(location.X, location.Y, location.Z, altRoom);
            if (!TestTriggerPlacement(level, location, altRoom, sector))
            {
                return false;
            }

            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(SC.FlipMapWarningMsg, level.Name, location.X, location.Y, location.Z, altRoom));
            }
        }

        return true;
    }

    private bool TestTriggerPlacement(TR3CombinedLevel level, Location location, short room, TRRoomSector sector)
    {
        if (!location.Validated && LocationUtilities.HasAnyTrigger(sector, level.Data.FloorData))
        {
            // There is already a trigger here and the location hasn't been marked as being
            // safe to move the action items to the new pickup trigger.
            if (Settings.DevelopmentMode)
            {
                Debug.WriteLine(string.Format(SC.InvalidLocationMsg, level.Name, location.X, location.Y, location.Z, room));
            }
            return false;
        }
        return true;
    }


    private void PlaceSecret(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret)
    {
        // This assumes TestTriggerPlacement has already been called and passed.
        TRRoomSector sector = level.Data.GetRoomSector(secret.Location);
        CreateSecretTriggers(level, secret, secret.Location.Room, sector);

        short altRoom = level.Data.Rooms[secret.Location.Room].AlternateRoom;
        if (altRoom != -1)
        {
            sector = level.Data.GetRoomSector(secret.Location.X, secret.Location.Y, secret.Location.Z, altRoom);
            CreateSecretTriggers(level, secret, altRoom, sector);
        }
    }

    private void CreateSecretTriggers(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret, short room, TRRoomSector baseSector)
    {
        // Try to make the primary trigger
        CreateSecretTrigger(level, secret, room, baseSector);

        // Check neighbouring sectors if we are very close to tile edges. We scan 8 locations around
        // the secret's position based on the edge tolerance and see if the sector has changed.
        HashSet<TRRoomSector> processedSectors = new() { baseSector };
        for (int xNorm = -1; xNorm < 2; xNorm++)
        {
            for (int zNorm = -1; zNorm < 2; zNorm++)
            {
                if (xNorm == 0 && zNorm == 0) continue; // Primary trigger's sector

                int x = secret.Location.X + xNorm * SC.TriggerEdgeLimit;
                int z = secret.Location.Z + zNorm * SC.TriggerEdgeLimit;
                TRRoomSector neighbour = level.Data.GetRoomSector(x, secret.Location.Y, z, room);

                // Process each unique sector only once and if it's a valid neighbour, add the extra trigger.
                // We test neighbouring sector heights as Lara doesn't clip up in TR3 unlike TR1 if she is
                // against the wall, so this avoids unnecessary extra FD.
                if (processedSectors.Add(neighbour)
                    && !IsInvalidNeighbour(baseSector, neighbour)
                    && Math.Abs(secret.Location.Y - LocationUtilities.GetCornerHeight(neighbour, level.Data.FloorData, x, z)) < TRConsts.Step1)
                {
                    CreateSecretTrigger(level, secret, room, neighbour);
                    if (Settings.DevelopmentMode)
                    {
                        Debug.WriteLine(string.Format(SC.EdgeInfoMsg, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                }
            }
        }
    }

    private static bool IsInvalidNeighbour(TRRoomSector baseSector, TRRoomSector neighbour)
    {
        return (neighbour.Floor == TRConsts.WallClicks && neighbour.Ceiling == TRConsts.WallClicks) // Inside a wall
            || (neighbour.RoomBelow != baseSector.RoomBelow)          // Mid-air
            ||
            (
                (neighbour.BoxIndex & 0x7FF0) >> 4 == 2047            // Neighbour is a slope
                && (baseSector.BoxIndex & 0x7FF0) >> 4 != 2047        // But the base sector isn't
            );
    }

    private void CreateSecretTrigger(TR3CombinedLevel level, TRSecretPlacement<TR3Type> secret, short room, TRRoomSector sector)
    {
        if (sector.FDIndex == 0)
        {
            level.Data.FloorData.CreateFloorData(sector);
        }

        // Make a new pickup trigger
        FDTriggerEntry trigger = new()
        {
            Mask = secret.TriggerMask,
            TrigType = FDTrigType.Pickup,
            Actions = new()
            {
                new()
                {
                    Parameter = secret.EntityIndex
                },
                new()
                {
                    Action = FDTrigAction.SecretFound,
                    Parameter = secret.SecretIndex
                }
            }
        };

        if (secret.TriggersDoor)
        {
            trigger.Actions.Add(new()
            {
                Parameter = secret.DoorIndex
            });
        }

        // Move any existing action list items to the new trigger and remove the old one. We can only
        // move Object actions if the mask on this trigger is full.
        if (level.Data.FloorData[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry existingTrigger)
        {
            List<FDActionItem> existingActions = new();
            foreach (FDActionItem actionItem in existingTrigger.Actions)
            {
                if (actionItem.Action == FDTrigAction.Object)
                {
                    if (Settings.DevelopmentMode)
                    {
                        existingActions.Add(actionItem); // Add it anyway for testing
                        Debug.WriteLine(string.Format(SC.TriggerWarningMsg, actionItem.Parameter, level.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, room));
                    }
                    else if (secret.TriggerMask == TRConsts.FullMask)
                    {
                        existingActions.Add(actionItem);
                    }
                }
                else
                {
                    existingActions.Add(actionItem);
                }
            }

            trigger.Actions.AddRange(existingActions);
            level.Data.FloorData[sector.FDIndex].Remove(existingTrigger);
        }

        level.Data.FloorData[sector.FDIndex].Add(trigger);
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
                    TR3ModelImporter importer = new()
                    {
                        Level = level.Data,
                        LevelName = level.Name,
                        EntitiesToImport = allocation.ImportModels,
                        DataFolder = _outer.GetResourcePath(@"TR3\Models"),
                        TexturePositionMonitor = monitor
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
                        monitor.EntityMap[artefactPickupType] = puzzlePickupType;
                        monitor.EntityMap[artefactMenuType] = puzzleMenuType;
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
                // Get rid of existing secret triggers
                RemoveDefaultSecrets(level);

                // Only create new secrets if applicable
                if (level.HasSecrets || _outer.Settings.DevelopmentMode)
                {
                    TRSecretModelAllocation<TR3Type> allocation = _importAllocations[level];

                    // Reward rooms can be conditionally chosen based on level state after placing secrets,
                    // but we need to make a placholder for door indices and masks to create those secrets.
                    TRSecretRoom<TR2Entity> rewardRoom = _outer.MakePlaceholderRewardRoom(level);

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
