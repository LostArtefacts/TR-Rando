using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TREnvironmentEditor;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Secrets;

namespace TRRandomizerCore.Randomizers
{
    public class TR3SecretRandomizer : BaseTR3Randomizer
    {
        private static readonly string _invalidLocationMsg = "Cannot place a nonvalidated secret where a trigger already exists - {0} [X={1}, Y={2}, Z={3}, R={4}]";
        private static readonly string _triggerWarningMsg = "Existing trigger object action with parameter {0} will be lost - {1} [X={2}, Y={3}, Z={4}, R={5}]";
        private static readonly List<int> _devRooms = null;
        private static readonly ushort _devModeSecretCount = 6;

        // Move this to Gamestring Rando once implemented
        private static readonly Dictionary<TR3Entities, string> _pickupNames = new Dictionary<TR3Entities, string>
        {
            [TR3Entities.Infada_P] = "Secret Infada Stone",
            [TR3Entities.OraDagger_P] = "Secret Ora Dagger",
            [TR3Entities.Element115_P] = "Secret Element 115",
            [TR3Entities.EyeOfIsis_P] = "Secret Eye of Isis"
        };

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Dictionary<string, List<Location>> locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\locations.json"));

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                // Get rid of existing secret triggers
                RemoveDefaultSecrets();

                // Only create new secrets if applicable
                if (Settings.DevelopmentMode || _levelInstance.HasSecrets)
                {
                    // Get the list of artefacts we can use as pickups
                    List<TR3Entities> pickupTypes = ImportArtefacts();

                    // Create the reward room. The returned list contains the indices
                    // of the doors leading into the room.
                    List<int> doorItems = MakeRewardRoom();

                    if (Settings.DevelopmentMode)
                    {
                        PlaceAllSecrets(locations[_levelInstance.Name], pickupTypes, doorItems);
                    }
                    else
                    {
                        RandomizeSecrets(locations[_levelInstance.Name], pickupTypes, doorItems);
                    }
                }

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RemoveDefaultSecrets()
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(_levelInstance.Data);

            // Scan all rooms and remove any existing secret triggers.
            foreach (TR3Room room in _levelInstance.Data.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    if (sector.FDIndex == 0)
                    {
                        continue;
                    }

                    List<FDEntry> entries = floorData.Entries[sector.FDIndex];
                    for (int i = entries.Count - 1; i >= 0; i--)
                    {
                        FDEntry entry = entries[i];
                        if (entry is FDTriggerEntry trig && trig.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null)
                        {
                            entries.RemoveAt(i);
                        }
                    }

                    if (entries.Count == 0)
                    {
                        // If there isn't anything left, reset the sector to point to the dummy FD
                        floorData.RemoveFloorData(sector);
                    }
                }
            }

            floorData.WriteToLevel(_levelInstance.Data);
        }

        private List<TR3Entities> ImportArtefacts()
        {
            Dictionary<TR3Entities, TR3Entities> artefactPickups = TR3EntityUtilities.GetArtefactPickups();
            Dictionary<TR3Entities, TR3Entities> artefactReplacements = TR3EntityUtilities.GetArtefactReplacements();

            // Work out which models are available to replace as secret pickups.
            // We exclude artefacts from import if this level already has an 
            // actual artefact model, and we exclude current puzzle/key items
            // from the available switching pool.
            List<TR3Entities> availableModels = new List<TR3Entities>();
            List<TRModel> models = _levelInstance.Data.Models.ToList();
            foreach (TR3Entities puzzleType in artefactReplacements.Keys)
            {
                if (models.Find(m => m.ID == (uint)puzzleType) == null)
                {
                    availableModels.Add(puzzleType);
                }
            }

            List<TR3Entities> artefactTypes = artefactPickups.Keys.ToList();
            for (int i = artefactTypes.Count - 1; i >= 0; i--)
            {
                TR3Entities artefactType = artefactTypes[i];
                if (models.Find(m => m.ID == (uint)artefactType) != null)
                {
                    artefactTypes.RemoveAt(i);
                }
            }

            // How many models do we actually need?
            ISet<TR3Entities> importModels = new SortedSet<TR3Entities>();

            int modelImportCount = Math.Min(Settings.DevelopmentMode ? _devModeSecretCount : _levelInstance.Script.NumSecrets, availableModels.Count);
            modelImportCount = Math.Min(modelImportCount, artefactTypes.Count);

            while (importModels.Count < modelImportCount)
            {
                importModels.Add(artefactTypes[_generator.Next(0, artefactTypes.Count)]);
            }

            if (importModels.Count > 0)
            {
                // Get the artefacts into the level and refresh the model list
                TR3ModelImporter importer = new TR3ModelImporter
                {
                    Level = _levelInstance.Data,
                    LevelName = _levelInstance.Name,
                    EntitiesToImport = importModels,
                    DataFolder = GetResourcePath(@"TR3\Models")
                };

                importer.Import();
                models = _levelInstance.Data.Models.ToList();
            }
            else
            {
                // The level already has all the models by default (e.g. Meteorite Cavern) so
                // the available list will be empty. We just add the models as being available
                // regardless because they are being re-assigned. The originals won't be affected.
                importModels = new SortedSet<TR3Entities>(artefactPickups.Keys);
            }

            // Stash the puzzle type models for returning so we know what to allocate
            // for the actual pickups. 
            List<TR3Entities> pickupModels = new List<TR3Entities>();

            // Redefine the artefacts as puzzle models otherwise the level ends on pickup
            foreach (TR3Entities artefactPickupType in importModels)
            {
                TR3Entities puzzlePickupType = availableModels.First();
                TR3Entities puzzleMenuType = artefactReplacements[puzzlePickupType];
                TR3Entities artefactMenuType = artefactPickups[artefactPickupType];

                models.Find(m => m.ID == (uint)artefactPickupType).ID = (uint)puzzlePickupType;
                models.Find(m => m.ID == (uint)artefactMenuType).ID = (uint)puzzleMenuType;

                // Remove this puzzle type from the available pool
                availableModels.RemoveAt(0);

                // Make the pickup type available to assign to items
                pickupModels.Add(puzzlePickupType);

                // Assign a name for the script
                string name = _pickupNames[artefactPickupType];
                if (TR3EntityUtilities.IsKeyType(puzzlePickupType))
                {
                    _levelInstance.Script.Keys[puzzlePickupType - TR3Entities.Key1_P] = name;
                }
                else if (TR3EntityUtilities.IsPuzzleType(puzzlePickupType))
                {
                    _levelInstance.Script.Puzzles[puzzlePickupType - TR3Entities.Puzzle1_P] = name;
                }
                else if (TR3EntityUtilities.IsQuestType(puzzlePickupType))
                {
                    _levelInstance.Script.Pickups[puzzlePickupType - TR3Entities.Quest1_P] = name;
                }
            }

            return pickupModels;
        }

        private List<int> MakeRewardRoom()
        {
            List<int> doorIndices = new List<int>();

            string mappingPath = @"TR3\SecretMapping\" + _levelInstance.Name + "-SecretMapping.json";
            if (ResourceExists(mappingPath))
            {
                TRSecretMapping<TR2Entity> secretMapping = JsonConvert.DeserializeObject<TRSecretMapping<TR2Entity>>(ReadResource(mappingPath), EMEditorMapping.Converter);

                // Select a reward room and create it.
                TRSecretRoom<TR2Entity> rewardRoom = secretMapping.Rooms[_generator.Next(0, secretMapping.Rooms.Count)];
                rewardRoom.Room.ApplyToLevel(_levelInstance.Data);

                // Trigger activation masks have 5 bits so we need a specific number of doors to match.
                // For development mode, test the maximum.
                double countedSecrets = Settings.DevelopmentMode ? _devModeSecretCount : _levelInstance.Script.NumSecrets;
                int requiredDoors = (int)Math.Ceiling(countedSecrets / TRSecretPlacement<TR3Entities>.MaskBits);
                if (rewardRoom.Doors.Count < requiredDoors)
                {
                    throw new Exception(string.Format("{0} secret doors required for {1}, but only {2} found.", requiredDoors, _levelInstance.Name, rewardRoom.Doors.Count));
                }

                // Make the doors and store the entity indices for the secret triggers
                List<TR2Entity> entities = _levelInstance.Data.Entities.ToList();
                for (int i = 0; i < requiredDoors; i++)
                {
                    TR2Entity door = rewardRoom.Doors[i];
                    if (door.Room == short.MaxValue)
                    {
                        door.Room = (short)(_levelInstance.Data.NumRooms - 1);
                    }
                    entities.Add(door);
                    doorIndices.Add((int)_levelInstance.Data.NumEntities);
                    _levelInstance.Data.NumEntities++;
                }

                _levelInstance.Data.Entities = entities.ToArray();

                // Spread the rewards out fairly evenly across each defined position in the new room.
                int rewardPositionCount = rewardRoom.RewardPositions.Count;
                for (int i = 0; i < secretMapping.RewardEntities.Count; i++)
                {
                    TR2Entity item = _levelInstance.Data.Entities[secretMapping.RewardEntities[i]];
                    Location position = rewardRoom.RewardPositions[i % rewardPositionCount];

                    item.X = position.X;
                    item.Y = position.Y;
                    item.Z = position.Z;
                    item.Room = (short)(_levelInstance.Data.NumRooms - 1);
                }
            }

            return doorIndices;
        }

        private void PlaceAllSecrets(List<Location> locations, List<TR3Entities> pickupTypes, List<int> doorItems)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(_levelInstance.Data);

            List<TR2Entity> entities = _levelInstance.Data.Entities.ToList();

            ushort secretIndex = 0;
            ushort countedSecrets = _devModeSecretCount; // For dev mode test the max number of secrets in TR3

            TRSecretPlacement<TR3Entities> secret = new TRSecretPlacement<TR3Entities>();
            foreach (Location location in locations)
            {
                if (_devRooms == null || _devRooms.Contains(location.Room))
                {
                    secret.Location = location;
                    secret.EntityIndex = (ushort)entities.Count;
                    secret.SecretIndex = secretIndex;
                    secret.PickupType = pickupTypes[0]; // Keep the same type in development mode

                    secret.SetMaskAndDoor(countedSecrets, doorItems);

                    TR2Entity secretEntity = PlaceSecret(secret, floorData);
                    if (secretEntity != null)
                    {
                        entities.Add(secretEntity);
                        if (++secretIndex == countedSecrets)
                        {
                            secretIndex = 0;
                        }
                    }
                }
            }

            _levelInstance.Data.Entities = entities.ToArray();
            _levelInstance.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(_levelInstance.Data);
        }

        private void RandomizeSecrets(List<Location> locations, List<TR3Entities> pickupTypes, List<int> doorItems)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(_levelInstance.Data);

            List<TR2Entity> entities = _levelInstance.Data.Entities.ToList();

            TRSecretPlacement<TR3Entities> secret = new TRSecretPlacement<TR3Entities>();
            List<Location> usedLocations = new List<Location>();
            while (secret.SecretIndex < _levelInstance.Script.NumSecrets)
            {
                // Placeholder until proximity zoning implemented
                Location location;
                do
                {
                    location = locations[_generator.Next(0, locations.Count)];
                }
                while
                (
                    usedLocations.Find(l => l.X == location.X && l.Y == location.Y && l.Z == location.Z && l.Room == location.Room) != null ||
                    (location.Difficulty == Difficulty.Hard && !Settings.HardSecrets) ||
                    (location.RequiresGlitch && !Settings.GlitchedSecrets)
                );

                usedLocations.Add(location);
                secret.Location = location;
                secret.EntityIndex = (ushort)entities.Count;
                secret.PickupType = pickupTypes[_generator.Next(0, pickupTypes.Count)];

                secret.SetMaskAndDoor(_levelInstance.Script.NumSecrets, doorItems);

                TR2Entity secretEntity = PlaceSecret(secret, floorData);
                if (secretEntity != null)
                {
                    entities.Add(secretEntity);
                    secret.SecretIndex++;
                }
            }

            _levelInstance.Data.Entities = entities.ToArray();
            _levelInstance.Data.NumEntities = (uint)entities.Count;

            floorData.WriteToLevel(_levelInstance.Data);
        }

        private TR2Entity PlaceSecret(TRSecretPlacement<TR3Entities> secret, FDControl floorData)
        {
            TR2Entity entity = new TR2Entity
            {
                TypeID = (short)secret.PickupType,
                X = secret.Location.X,
                Y = secret.Location.Y,
                Z = secret.Location.Z,
                Room = (short)secret.Location.Room,
                Angle = 0,
                Intensity1 = -1,
                Intensity2 = -1
            };

            // Get the sector and create the FD if required
            TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, entity.Room, _levelInstance.Data, floorData);
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            FDTriggerEntry existingTrigger = floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            if (existingTrigger != null && !secret.Location.Validated)
            {
                // There is already a trigger here and the location hasn't been marked as being
                // safe to move the action items to the new pickup trigger.
                if (Settings.DevelopmentMode)
                {
                    Debug.WriteLine(string.Format(_invalidLocationMsg, _levelInstance.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                }
                return null;
            }

            // Make a new pickup trigger
            FDTriggerEntry trigger = new FDTriggerEntry
            {
                Setup = new FDSetup { Value = 4 },
                TrigSetup = new FDTrigSetup
                {
                    Value = 15872,
                    Mask = secret.TriggerMask
                },
                TrigType = FDTrigType.Pickup,
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.Object,
                        Parameter = secret.EntityIndex
                    },
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.SecretFound,
                        Parameter = secret.SecretIndex
                    }
                }
            };

            if (secret.TriggersDoor)
            {
                trigger.TrigActionList.Add(new FDActionListItem
                {
                    TrigAction = FDTrigAction.Object,
                    Parameter = secret.DoorIndex
                });
            }

            // Move any existing action list items to the new trigger and remove the old one. We can only
            // move Object actions if the mask on this trigger is full.
            if (existingTrigger != null)
            {
                List<FDActionListItem> existingActions = new List<FDActionListItem>();
                foreach (FDActionListItem actionItem in existingTrigger.TrigActionList)
                {
                    if (actionItem.TrigAction == FDTrigAction.Object)
                    {
                        if (Settings.DevelopmentMode)
                        {
                            existingActions.Add(actionItem); // Add it anyway for testing
                            Debug.WriteLine(string.Format(_triggerWarningMsg, actionItem.Parameter, _levelInstance.Name, secret.Location.X, secret.Location.Y, secret.Location.Z, secret.Location.Room));
                        }
                        else if (secret.TriggerMask == TRSecretPlacement<TR3Entities>.FullActivation)
                        {
                            existingActions.Add(actionItem);
                        }
                    }
                    else
                    {
                        existingActions.Add(actionItem);
                    }
                }

                trigger.TrigActionList.AddRange(existingActions);
                floorData.Entries[sector.FDIndex].Remove(existingTrigger);
            }

            floorData.Entries[sector.FDIndex].Add(trigger);

            return entity;
        }
    }
}