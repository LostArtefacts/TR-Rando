using Newtonsoft.Json;
using System.Numerics;
using TRDataControl.Environment;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1EnemyAllocator : EnemyAllocator<TR1Type>
{
    private static readonly EnemyTransportCollection<TR1Type> _emptyEnemies = new();

    private static readonly int _unkillableEgyptMummy = 163;
    private static readonly Location _egyptMummyLocation = new()
    {
        X = 66048,
        Y = -2304,
        Z = 73216,
        Room = 78
    };

    private static readonly int _unreachableStrongholdRoom = 18;
    private static readonly Location _strongholdCentaurLocation = new()
    {
        X = 57856,
        Y = -26880,
        Z = 43520,
        Room = 14
    };

    private static readonly double _emptyEggChance = 0.25;
    private static readonly double _mummyWingChance = 0.5;

    private readonly Dictionary<string, List<Location>> _pistolLocations;
    private readonly Dictionary<string, List<Location>> _eggLocations;
    private readonly Dictionary<string, List<Location>> _pierreLocations;
    
    public ItemFactory<TR1Entity> ItemFactory { get; set; }

    public TR1EnemyAllocator()
    {
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR1\Locations\unarmed_locations.json"));
        _eggLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR1\Locations\egg_locations.json"));
        _pierreLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR1\Locations\pierre_locations.json"));
    }

    protected override Dictionary<TR1Type, List<string>> GetGameTracker()
        => TR1EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty, GameLevels);

    protected override bool IsEnemySupported(string levelName, TR1Type type, RandoDifficulty difficulty)
        => TR1EnemyUtilities.IsEnemySupported(levelName, type, difficulty);

    protected override Dictionary<TR1Type, List<int>> GetRestrictedRooms(string levelName, RandoDifficulty difficulty)
        => TR1EnemyUtilities.GetRestrictedEnemyRooms(levelName, RandoDifficulty.Default);

    protected override bool IsOneShotType(TR1Type type)
        => type == TR1Type.Pierre;

    public EnemyTransportCollection<TR1Type> SelectCrossLevelEnemies(string levelName, TR1Level level)
    {
        if (levelName == TR1LevelNames.ASSAULT)
        {
            return null;
        }

        AdjustUnkillableEnemies(levelName, level);

        if (Settings.UseEnemyClones && Settings.CloneOriginalEnemies)
        {
            // Skip import altogether for OG clone mode
            return _emptyEnemies;
        }

        // If level-ending Larson is disabled, we make an alternative ending to ToQ.
        // Do this at this stage as it effectively gets rid of ToQ-Larson meaning
        // Sanctuary-Larson can potentially be imported.
        if (levelName == TR1LevelNames.QUALOPEC && Settings.ReplaceRequiredEnemies)
        {
            AmendToQLarson(level);
        }

        if (TR1LevelNames.AsListGold.Contains(levelName))
        {
            // Ensure big eggs are randomized by converting to normal ones because
            // big eggs are never part of the enemy pool.
            level.Entities.FindAll(e => e.TypeID == TR1Type.AdamEgg)
                .ForEach(e => e.TypeID = TR1Type.AtlanteanEgg);
        }

        RandoDifficulty difficulty = GetImpliedDifficulty();

        List<TR1Type> oldTypes = GetCurrentEnemyEntities(level);
        List<TR1Type> allEnemies = TR1TypeUtilities.GetCandidateCrossLevelEnemies();

        int enemyCount = oldTypes.Count + TR1EnemyUtilities.GetEnemyAdjustmentCount(levelName);
        if (levelName == TR1LevelNames.QUALOPEC && Settings.ReplaceRequiredEnemies)
        {
            // Account for Larson having been removed above.
            ++enemyCount;
        }
        List<TR1Type> newTypes = new(enemyCount);

        // TR1 doesn't kill land creatures when underwater, so if "no restrictions" is
        // enabled, don't enforce any by default.
        bool waterEnemyRequired = difficulty == RandoDifficulty.Default
            && TR1TypeUtilities.GetWaterEnemies().Any(oldTypes.Contains);

        if (waterEnemyRequired)
        {
            List<TR1Type> waterEnemies = TR1TypeUtilities.GetWaterEnemies();
            newTypes.Add(SelectRequiredEnemy(waterEnemies, levelName, difficulty));
        }

        if (!Settings.ReplaceRequiredEnemies)
        {
            foreach (TR1Type type in TR1EnemyUtilities.GetRequiredEnemies(levelName))
            {
                if (!newTypes.Contains(type))
                {
                    newTypes.Add(type);
                }
            }
        }

        // Remove all exclusions from the pool, and adjust the target capacity
        allEnemies.RemoveAll(_excludedEnemies.Contains);

        IEnumerable<TR1Type> ex = allEnemies.Where(e => !newTypes.Any(TR1TypeUtilities.GetFamily(e).Contains));
        List<TR1Type> unalisedTypes = TR1TypeUtilities.RemoveAliases(ex);
        while (unalisedTypes.Count < newTypes.Capacity - newTypes.Count)
        {
            --newTypes.Capacity;
        }

        // Fill the remainder to capacity as randomly as we can
        HashSet<TR1Type> testedTypes = new();
        List<TR1Type> eggTypes = TR1TypeUtilities.GetAtlanteanEggEnemies();
        while (newTypes.Count < newTypes.Capacity && testedTypes.Count < allEnemies.Count)
        {
            TR1Type type = allEnemies[Generator.Next(0, allEnemies.Count)];
            testedTypes.Add(type);

            if (!TR1EnemyUtilities.IsEnemySupported(levelName, type, difficulty))
            {
                continue;
            }

            // Grounded Atlanteans require the flyer for meshes so we can't have a grounded mummy and meaty flyer, or vice versa as a result.
            if (type == TR1Type.BandagedAtlantean && newTypes.Contains(TR1Type.MeatyFlyer) && !newTypes.Contains(TR1Type.MeatyAtlantean))
            {
                type = TR1Type.MeatyAtlantean;
            }
            else if (type == TR1Type.MeatyAtlantean && newTypes.Contains(TR1Type.BandagedFlyer) && !newTypes.Contains(TR1Type.BandagedAtlantean))
            {
                type = TR1Type.BandagedAtlantean;
            }
            else if (type == TR1Type.BandagedFlyer && newTypes.Contains(TR1Type.MeatyAtlantean))
            {
                continue;
            }
            else if (type == TR1Type.MeatyFlyer && newTypes.Contains(TR1Type.BandagedAtlantean))
            {
                continue;
            }
            else if (type == TR1Type.AtlanteanEgg && !newTypes.Any(eggTypes.Contains))
            {
                List<TR1Type> preferredEggTypes = eggTypes.FindAll(allEnemies.Contains);
                if (preferredEggTypes.Count == 0)
                {
                    preferredEggTypes = eggTypes;
                }
                TR1Type eggType = preferredEggTypes[Generator.Next(0, preferredEggTypes.Count)];
                newTypes.Add(eggType);
                testedTypes.Add(eggType);
            }

            // If this is a tracked enemy throughout the game, we only allow it if the number
            // of unique levels is within the limit. Bear in mind we are collecting more than
            // one group of enemies per level.
            if (_gameEnemyTracker.ContainsKey(type) && !_gameEnemyTracker[type].Contains(levelName))
            {
                if (_gameEnemyTracker[type].Count < _gameEnemyTracker[type].Capacity)
                {
                    _gameEnemyTracker[type].Add(levelName);
                }
                else
                {
                    // If we tried to previously exclude this enemy and couldn't, it will slip
                    // through the net and so the appearances will increase.
                    if (allEnemies.Except(newTypes).Count() > 1)
                    {
                        continue;
                    }
                }
            }

            List<TR1Type> family = TR1TypeUtilities.GetFamily(type);
            if (!newTypes.Any(family.Contains))
            {
                newTypes.Add(type);
            }
        }

        if
        (
            newTypes.All(e => TR1TypeUtilities.IsWaterCreature(e) || TR1EnemyUtilities.IsEnemyRestricted(levelName, e, difficulty)) ||
            (newTypes.Capacity > 1 && newTypes.All(e => TR1EnemyUtilities.IsEnemyRestricted(levelName, e, difficulty)))
        )
        {
            // Make sure we have an unrestricted enemy available for the individual level conditions. This will
            // guarantee a "safe" enemy for the level; we avoid aliases here to avoid further complication.
            bool RestrictionCheck(TR1Type e) =>
                !TR1EnemyUtilities.IsEnemySupported(levelName, e, difficulty)
                || newTypes.Contains(e)
                || TR1TypeUtilities.IsWaterCreature(e)
                || TR1EnemyUtilities.IsEnemyRestricted(levelName, e, difficulty)
                || TR1TypeUtilities.TranslateAlias(e) != e;

            List<TR1Type> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
            if (unrestrictedPool.Count == 0)
            {
                // We are going to have to pull in the full list of candidates again, so ignoring any exclusions
                unrestrictedPool = TR1TypeUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
            }

            TR1Type type = unrestrictedPool[Generator.Next(0, unrestrictedPool.Count)];
            newTypes.Add(type);

            if (type == TR1Type.AtlanteanEgg && !newTypes.Any(eggTypes.Contains))
            {
                List<TR1Type> preferredEggTypes = eggTypes.FindAll(allEnemies.Contains);
                if (preferredEggTypes.Count == 0)
                {
                    preferredEggTypes = eggTypes;
                }
                TR1Type eggType = preferredEggTypes[Generator.Next(0, preferredEggTypes.Count)];
                newTypes.Add(eggType);
            }
        }

        if (levelName == TR1LevelNames.PYRAMID && Settings.ReplaceRequiredEnemies && !newTypes.Contains(TR1Type.Adam))
        {
            AmendPyramidTorso(level);
        }

        return new()
        {
            TypesToImport = newTypes,
            TypesToRemove = oldTypes
        };
    }

    private static List<TR1Type> GetCurrentEnemyEntities(TR1Level level)
    {
        List<TR1Type> allGameEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        SortedSet<TR1Type> allLevelEnts = new(level.Entities.Select(e => e.TypeID));
        return allLevelEnts.Where(allGameEnemies.Contains).ToList();
    }

    public EnemyRandomizationCollection<TR1Type> RandomizeEnemiesNatively(string levelName, TR1Level level)
    {
        if (levelName == TR1LevelNames.ASSAULT)
        {
            return null;
        }

        AdjustUnkillableEnemies(levelName, level);
        EnemyRandomizationCollection<TR1Type> enemies = new()
        {
            Available = new(),
            Water = new()
        };

        if (!Settings.UseEnemyClones || !Settings.CloneOriginalEnemies)
        {
            enemies.Available.AddRange(GetCurrentEnemyEntities(level));
            enemies.Water.AddRange(TR1TypeUtilities.FilterWaterEnemies(enemies.Available));
        }

        RandomizeEnemies(levelName, level, enemies);

        return enemies;
    }

    public void RandomizeEnemies(string levelName, TR1Level level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        AmendAtlanteanModels(level, enemies);

        // Get a list of current enemy entities
        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> enemyEntities = level.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        RandoDifficulty difficulty = GetImpliedDifficulty();

        // First iterate through any enemies that are restricted by room
        Dictionary<TR1Type, List<int>> enemyRooms = TR1EnemyUtilities.GetRestrictedEnemyRooms(levelName, difficulty);
        if (enemyRooms != null)
        {
            foreach (TR1Type type in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(type))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[type];
                int maxEntityCount = TR1EnemyUtilities.GetRestrictedEnemyLevelCount(type, difficulty);
                if (maxEntityCount == -1)
                {
                    // We are allowed any number, but this can't be more than the number of unique rooms,
                    // so we will assume 1 per room as these restricted enemies are likely to be tanky.
                    maxEntityCount = rooms.Count;
                }
                else
                {
                    maxEntityCount = Math.Min(maxEntityCount, rooms.Count);
                }

                // Pick an actual count
                int enemyCount = Generator.Next(1, maxEntityCount + 1);
                for (int i = 0; i < enemyCount; i++)
                {
                    // Find an entity in one of the rooms that the new enemy is restricted to
                    TR1Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[type][Generator.Next(0, enemyRooms[type].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR1TypeUtilities.IsWaterCreature(type) && level.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    targetEntity.TypeID = TR1TypeUtilities.TranslateAlias(type);
                    SetOneShot(targetEntity, level.Entities.IndexOf(targetEntity), level.FloorData);
                    enemyEntities.Remove(targetEntity);

                    if (Settings.HideEnemiesUntilTriggered || type == TR1Type.Adam)
                    {
                        targetEntity.Invisible = true;
                    }
                }

                enemies.Available.Remove(type);
            }
        }

        foreach (TR1Entity currentEntity in enemyEntities)
        {
            if (enemies.Available.Count == 0)
            {
                continue;
            }

            int entityIndex = level.Entities.IndexOf(currentEntity);
            TR1Type currentType = currentEntity.TypeID;
            TR1Type newType = currentType;

            // If it's an existing enemy that has to remain in the same spot, skip it
            if (!Settings.ReplaceRequiredEnemies && TR1EnemyUtilities.IsEnemyRequired(levelName, currentType))
            {
                _resultantEnemies.Add(currentType);
                continue;
            }

            List<TR1Type> enemyPool;
            if (difficulty == RandoDifficulty.Default && IsEnemyInOrAboveWater(currentEntity, level))
            {
                // Make sure we replace with another water enemy
                enemyPool = enemies.Water;
            }
            else
            {
                // Otherwise we can pick any other available enemy
                enemyPool = enemies.Available;
            }

            // Pick a new type
            newType = enemyPool[Generator.Next(0, enemyPool.Count)];

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Kold, SkateboardKid).
            int maxEntityCount = TR1EnemyUtilities.GetRestrictedEnemyLevelCount(newType, difficulty);
            if (maxEntityCount != -1)
            {
                if (GetEntityCount(level, newType) >= maxEntityCount)
                {
                    List<TR1Type> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(levelName, TR1TypeUtilities.TranslateAlias(e)));
                    if (pool.Count > 0)
                    {
                        newType = pool[Generator.Next(0, pool.Count)];
                    }
                }
            }

            // Rather than individual enemy limits, this accounts for enemy groups such as all Atlanteans
            RandoDifficulty groupDifficulty = difficulty;
            if (levelName == TR1LevelNames.QUALOPEC && newType == TR1Type.Larson && Settings.ReplaceRequiredEnemies)
            {
                // Non-level ending Larson is not restricted in ToQ, otherwise we adhere to the normal rules.
                groupDifficulty = RandoDifficulty.NoRestrictions;
            }
            RestrictedEnemyGroup enemyGroup = TR1EnemyUtilities.GetRestrictedEnemyGroup(levelName, TR1TypeUtilities.TranslateAlias(newType), groupDifficulty);
            if (enemyGroup != null)
            {
                if (level.Entities.FindAll(e => enemyGroup.Enemies.Contains(e.TypeID)).Count >= enemyGroup.MaximumCount)
                {
                    List<TR1Type> pool = enemyPool.FindAll(e => !TR1EnemyUtilities.IsEnemyRestricted(levelName, TR1TypeUtilities.TranslateAlias(e), groupDifficulty));
                    if (pool.Count > 0)
                    {
                        newType = pool[Generator.Next(0, pool.Count)];
                    }
                }
            }

            // Tomp1 switches rats/crocs automatically if a room is flooded or drained. But we may have added a normal
            // land enemy to a room that eventually gets flooded. So in default difficulty, ensure the entity is a
            // hybrid, otherwise allow land creatures underwater (which works, but is obviously more difficult).
            if (difficulty == RandoDifficulty.Default)
            {
                TR1Room currentRoom = level.Rooms[currentEntity.Room];
                if (currentRoom.AlternateRoom != -1 
                    && level.Rooms[currentRoom.AlternateRoom].ContainsWater 
                    && TR1TypeUtilities.IsWaterLandCreatureEquivalent(currentType)
                    && !TR1TypeUtilities.IsWaterLandCreatureEquivalent(newType))
                {
                    Dictionary<TR1Type, TR1Type> hybrids = TR1TypeUtilities.GetWaterEnemyLandCreatures();
                    List<TR1Type> pool = enemies.Available.FindAll(e => hybrids.ContainsKey(e) || hybrids.ContainsValue(e));
                    if (pool.Count > 0)
                    {
                        newType = TR1TypeUtilities.GetWaterEnemyLandCreature(pool[Generator.Next(0, pool.Count)]);
                    }
                }
            }

            if (Settings.HideEnemiesUntilTriggered)
            {
                // Default to hiding the enemy - checks below for eggs, ex-eggs, Adam and centaur
                // statues will override as necessary.
                currentEntity.Invisible = true;
            }

            if (newType == TR1Type.AtlanteanEgg)
            {
                List<TR1Type> allEggTypes = TR1TypeUtilities.GetAtlanteanEggEnemies();
                List<TR1Type> spawnTypes = enemies.Available.FindAll(allEggTypes.Contains);
                TR1Type spawnType = TR1TypeUtilities.TranslateAlias(spawnTypes[Generator.Next(0, spawnTypes.Count)]);

                Location eggLocation = _eggLocations.ContainsKey(levelName)
                    ? _eggLocations[levelName].Find(l => l.EntityIndex == entityIndex)
                    : null;

                if (eggLocation != null || currentType == newType)
                {
                    if (Settings.AllowEmptyEggs)
                    {
                        // We can add Adam to make it possible for a dud spawn - he's not normally available for eggs because
                        // of his own restrictions.
                        if (!level.Models.ContainsKey(TR1Type.Adam))
                        {
                            allEggTypes.Add(TR1Type.Adam);
                        }

                        if (!allEggTypes.All(e => level.Models.ContainsKey(TR1TypeUtilities.TranslateAlias(e))) && Generator.NextDouble() < _emptyEggChance)
                        {
                            do
                            {
                                spawnType = TR1TypeUtilities.TranslateAlias(allEggTypes[Generator.Next(0, allEggTypes.Count)]);
                            }
                            while (level.Models.ContainsKey(spawnType));
                        }
                    }

                    currentEntity.CodeBits = TR1EnemyUtilities.AtlanteanToCodeBits(spawnType);
                    if (eggLocation != null)
                    {
                        currentEntity.SetLocation(eggLocation);
                    }

                    // Eggs will always be visible
                    currentEntity.Invisible = false;
                }
                else
                {
                    // We don't want an egg for this particular enemy, so just make it spawn as the actual type
                    newType = spawnType;
                }
            }
            else if (currentType == TR1Type.AtlanteanEgg)
            {
                // Hide what used to be eggs and reset the CodeBits otherwise this can interfere with trigger masks.
                currentEntity.Invisible = true;
                currentEntity.CodeBits = 0;
            }

            if (newType == TR1Type.CentaurStatue)
            {
                AdjustCentaurStatue(currentEntity, level);
            }
            else if (newType == TR1Type.Adam)
            {
                // Adam should always be invisible as he is inactive high above the ground
                // so this can interfere with Lara's route - see Cistern item 36
                currentEntity.Invisible = true;
            }
            else if (newType == TR1Type.Pierre
                && _pierreLocations.ContainsKey(levelName)
                && _pierreLocations[levelName].Find(l => l.EntityIndex == entityIndex) is Location location)
            {
                // Pierre is the only enemy who cannot be underwater, so location shifts have been predefined
                // for specific entities.
                currentEntity.SetLocation(location);
            }

            // Final step is to convert/set the type and ensure OneShot is set if needed (#146)
            currentEntity.TypeID = TR1TypeUtilities.TranslateAlias(newType);
            SetOneShot(currentEntity, entityIndex, level.FloorData);
            _resultantEnemies.Add(newType);
        }
    }

    private static int GetEntityCount(TR1Level level, TR1Type entityType)
    {
        int count = 0;
        TR1Type translatedType = TR1TypeUtilities.TranslateAlias(entityType);
        foreach (TR1Entity entity in level.Entities)
        {
            TR1Type type = entity.TypeID;
            if (type == translatedType)
            {
                count++;
            }
            else if (type == TR1Type.AdamEgg || type == TR1Type.AtlanteanEgg)
            {
                TR1Type eggType = TR1EnemyUtilities.CodeBitsToAtlantean(entity.CodeBits);
                if (eggType == translatedType && level.Models.ContainsKey(eggType))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private static bool IsEnemyInOrAboveWater(TR1Entity entity, TR1Level level)
    {
        if (level.Rooms[entity.Room].ContainsWater)
        {
            return true;
        }

        // Example where we have to search is Midas room 21
        TRRoomSector sector = level.GetRoomSector(entity.X, entity.Y - TRConsts.Step1, entity.Z, entity.Room);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            if (level.Rooms[sector.RoomBelow].ContainsWater)
            {
                return true;
            }
            sector = level.GetRoomSector(entity.X, (sector.Floor + 1) * TRConsts.Step1, entity.Z, sector.RoomBelow);
        }
        return false;
    }

    private static void AdjustCentaurStatue(TR1Entity entity, TR1Level level)
    {
        // If they're floating, they tend not to trigger as Lara's not within range
        TR1LocationGenerator locationGenerator = new();

        int y = entity.Y;
        short room = entity.Room;
        TRRoomSector sector = level.GetRoomSector(entity.X, y, entity.Z, room);
        while (sector.RoomBelow != TRConsts.NoRoom)
        {
            y = (sector.Floor + 1) * TRConsts.Step1;
            room = sector.RoomBelow;
            sector = level.GetRoomSector(entity.X, y, entity.Z, room);
        }

        entity.Y = sector.Floor * TRConsts.Step1;
        entity.Room = room;

        // Change this GetHeight
        if (sector.FDIndex != 0)
        {
            FDEntry entry = level.FloorData[sector.FDIndex].Find(e => e is FDSlantEntry s && s.Type == FDSlantType.Floor);
            if (entry is FDSlantEntry slant)
            {
                Vector4? bestMidpoint = locationGenerator.GetBestSlantMidpoint(slant);
                if (bestMidpoint.HasValue)
                {
                    entity.Y += (int)bestMidpoint.Value.Y;
                }
            }
        }

        entity.Invisible = false;
    }

    public void AdjustUnkillableEnemies(string levelName, TR1Level level)
    {
        if (levelName == TR1LevelNames.EGYPT)
        {
            // The OG mummy normally falls out of sight when triggered, so move it.
            level.Entities[_unkillableEgyptMummy].SetLocation(_egyptMummyLocation);
        }
        else if (levelName == TR1LevelNames.STRONGHOLD)
        {
            // There is a triggered centaur in room 18, plus several untriggered eggs for show.
            // Move the centaur, and free the eggs to be repurposed elsewhere.
            foreach (TR1Entity enemy in level.Entities.Where(e => e.Room == _unreachableStrongholdRoom))
            {
                int index = level.Entities.IndexOf(enemy);
                if (level.FloorData.GetEntityTriggers(index).Count == 0)
                {
                    enemy.TypeID = TR1Type.CameraTarget_N;
                    ItemFactory.FreeItem(levelName, index);
                }
                else
                {
                    enemy.SetLocation(_strongholdCentaurLocation);
                }
            }
        }
    }

    private static void AmendToQLarson(TR1Level level)
    {
        // Convert the Larson model into the Great Pyramid scion to allow ending the level. Larson will
        // become a raptor to allow for normal randomization. Environment mods will handle the specifics here.
        if (!level.Models.ChangeKey(TR1Type.Larson, TR1Type.ScionPiece3_S_P))
        {
            return;
        }

        level.Entities
            .FindAll(e => e.TypeID == TR1Type.Larson)
            .ForEach(e => e.TypeID = TR1Type.Raptor);

        // Make the scion invisible.
        MeshEditor editor = new();
        foreach (TRMesh mesh in level.Models[TR1Type.ScionPiece3_S_P].Meshes)
        {
            editor.Mesh = mesh;
            editor.ClearAllPolygons();
        }
    }

    private void AmendPyramidTorso(TR1Level level)
    {
        // We want to keep Adam's egg, but simulate something else hatching.
        // In hard mode, two enemies take his place.
        level.Models.Remove(TR1Type.Adam);

        TR1Entity egg = level.Entities.Find(e => e.TypeID == TR1Type.AdamEgg);
        TR1Entity lara = level.Entities.Find(e => e.TypeID == TR1Type.Lara);

        EMAppendTriggerActionFunction trigFunc = new()
        {
            Location = new()
            {
                X = lara.X,
                Y = lara.Y,
                Z = lara.Z,
                Room = lara.Room
            },
            Actions = new()
        };

        int count = Settings.RandoEnemyDifficulty == RandoDifficulty.Default ? 1 : 2;
        for (int i = 0; i < count; i++)
        {
            trigFunc.Actions.Add(new()
            {
                Parameter = (short)level.Entities.Count
            });

            level.Entities.Add(new()
            {
                TypeID = TR1Type.Adam,
                X = egg.X,
                Y = egg.Y - i * TRConsts.Step4,
                Z = egg.Z - TRConsts.Step4,
                Room = egg.Room,
                Angle = egg.Angle,
                Intensity = egg.Intensity,
                Invisible = true
            });
        }

        trigFunc.ApplyToLevel(level);
    }

    private void AmendAtlanteanModels(TR1Level level, EnemyRandomizationCollection<TR1Type> enemies)
    {
        // If non-shooting grounded Atlanteans are present, we can just duplicate the model to make shooting Atlanteans
        if (enemies.Available.Any(TR1TypeUtilities.GetFamily(TR1Type.ShootingAtlantean_N).Contains))
        {
            TRModel shooter = level.Models[TR1Type.ShootingAtlantean_N];
            TRModel nonShooter = level.Models[TR1Type.NonShootingAtlantean_N];
            if (shooter == null && nonShooter != null)
            {
                shooter = nonShooter.Clone();
                level.Models[TR1Type.ShootingAtlantean_N] = shooter;
                enemies.Available.Add(TR1Type.ShootingAtlantean_N);
            }
        }

        // If we're using flying mummies, add a chance that they'll have proper wings
        if (enemies.Available.Contains(TR1Type.BandagedFlyer) && Generator.NextDouble() < _mummyWingChance)
        {
            List<TRMesh> meshes = level.Models[TR1Type.FlyingAtlantean].Meshes;
            ushort bandageTexture = meshes[1].TexturedRectangles[3].Texture;
            for (int i = 15; i < 21; i++)
            {
                TRMesh mesh = meshes[i];
                foreach (TRMeshFace face in mesh.TexturedFaces)
                {
                    face.Texture = bandageTexture;
                }
            }
        }
    }

    public void AddUnarmedLevelAmmo(string levelName, TR1Level level, Action<Location, TR1Type> createItemCallback)
    {
        if (!Settings.CrossLevelEnemies || !Settings.GiveUnarmedItems)
        {
            return;
        }

        // Find out which gun we have for this level
        List<TR1Type> weaponTypes = TR1TypeUtilities.GetWeaponPickups();
        TR1Entity weaponEntity = level.Entities.Find(e =>
            weaponTypes.Contains(e.TypeID)
            && _pistolLocations[levelName].Any(l => l.IsEquivalent(e.GetLocation())));

        if (weaponEntity == null)
        {
            return;
        }

        Location weaponLocation = weaponEntity.GetLocation();

        List<TR1Type> allEnemies = TR1TypeUtilities.GetFullListOfEnemies();
        List<TR1Entity> levelEnemies = level.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        for (int i = 0; i < level.Entities.Count; i++)
        {
            TR1Entity entity = level.Entities[i];
            if ((entity.TypeID == TR1Type.AtlanteanEgg || entity.TypeID == TR1Type.AdamEgg)
                && level.FloorData.GetEntityTriggers(i).Any())
            {
                TR1Entity resultantEnemy = new()
                {
                    TypeID = TR1EnemyUtilities.CodeBitsToAtlantean(entity.CodeBits)
                };

                if (level.Models.ContainsKey(resultantEnemy.TypeID))
                {
                    levelEnemies.Add(resultantEnemy);
                }
            }
        }

        EnemyDifficulty difficulty = TR1EnemyUtilities.GetEnemyDifficulty(levelEnemies);

        if (difficulty > EnemyDifficulty.Medium
            && !level.Entities.Any(e => e.TypeID == TR1Type.Pistols_S_P))
        {
            createItemCallback(weaponLocation, TR1Type.Pistols_S_P);
        }

        if (difficulty > EnemyDifficulty.Easy)
        {
            while (weaponEntity.TypeID == TR1Type.Pistols_S_P)
            {
                weaponEntity.TypeID = weaponTypes[Generator.Next(0, weaponTypes.Count)];
            }
        }

        TR1Type weaponType = weaponEntity.TypeID;
        int ammoAllocation = TR1EnemyUtilities.GetStartingAmmo(weaponType);
        if (ammoAllocation > 0)
        {
            ammoAllocation *= (int)difficulty;
            TR1Type ammoType = TR1TypeUtilities.GetWeaponAmmo(weaponType);
            for (int i = 0; i < ammoAllocation; i++)
            {
                createItemCallback(weaponLocation, ammoType);
            }
        }

        if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
        {
            createItemCallback(weaponLocation, TR1Type.SmallMed_S_P);
            createItemCallback(weaponLocation, TR1Type.LargeMed_S_P);
        }
        if (difficulty > EnemyDifficulty.Medium)
        {
            createItemCallback(weaponLocation, TR1Type.LargeMed_S_P);
        }
        if (difficulty == EnemyDifficulty.VeryHard)
        {
            createItemCallback(weaponLocation, TR1Type.LargeMed_S_P);
        }
    }
}
