using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2EnemyAllocator : EnemyAllocator<TR2Type>
{
    private const int _friendlyEnemyLimit = 20;
    private const int _hshPlaceholderCount = 15;
    private const int _platformEndRoom = 77;

    private static readonly EnemyTransportCollection<TR2Type> _emptyEnemies = new();
    private static readonly List<int> _floaterFlameEnemies = new() { 34, 35 };
    private static readonly TR2Entity _hshPlaceholderDog = new()
    {
        TypeID = TR2Type.Doberman,
        X = 61952,
        Y = 2560,
        Z = 74240,
        Room = 85,
        Invisible = true,
    };

    private static readonly List<string> _friendlyLimitLevels = new()
    {
        TR2LevelNames.OPERA,
        TR2LevelNames.MONASTERY,
    };

    public TR2EnemyAllocator()
        : base(TRGameVersion.TR2) { }

    public List<string> DragonLevels { get; set; }
    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    protected override Dictionary<TR2Type, List<string>> GetGameTracker()
        => TR2EnemyUtilities.PrepareEnemyGameTracker(Settings.DocileChickens, Settings.RandoEnemyDifficulty);

    protected override bool IsEnemySupported(string levelName, TR2Type type, RandoDifficulty difficulty)
        => TR2EnemyUtilities.IsEnemySupported(levelName, type, difficulty, Settings.ProtectMonks, Settings.IsRemastered);

    protected override Dictionary<TR2Type, List<int>> GetRestrictedRooms(string levelName, RandoDifficulty difficulty)
        => TR2EnemyUtilities.GetRestrictedEnemyRooms(levelName, RandoDifficulty.Default);

    protected override bool IsOneShotType(TR2Type type)
        => type == TR2Type.MarcoBartoli;

    public EnemyTransportCollection<TR2Type> SelectCrossLevelEnemies(string levelName, TR2Level level)
    {
        if (levelName == TR2LevelNames.ASSAULT)
        {
            return null;
        }

        if (!Settings.IsRemastered && Settings.UseEnemyClones && Settings.CloneOriginalEnemies)
        {
            // Skip import altogether for OG clone mode
            return _emptyEnemies;
        }

        List<TR2Type> oldTypes = GetCurrentEnemyTypes(level);
        int enemyCount = oldTypes.Count + TR2EnemyUtilities.GetEnemyAdjustmentCount(levelName, Settings.IsRemastered);
        List<TR2Type> newTypes = new(enemyCount);

        List<TR2Type> chickenGuisers = TR2EnemyUtilities.GetEnemyGuisers(TR2Type.BirdMonster);
        TR2Type chickenGuiser = TR2Type.BirdMonster;

        RandoDifficulty difficulty = GetImpliedDifficulty();

        if (TR2EnemyUtilities.IsWaterEnemyRequired(level))
        {
            List<TR2Type> waterEnemies = TR2TypeUtilities.KillableWaterCreatures(Settings.IsRemastered);
            newTypes.Add(SelectRequiredEnemy(waterEnemies, levelName, difficulty));
        }

        if (TR2EnemyUtilities.IsDroppableEnemyRequired(level)
            && !newTypes.Any(t => TR2TypeUtilities.CanDropPickups(t, !Settings.ProtectMonks, Settings.UnconditionalChickens, Settings.IsRemastered)))
        {
            List<TR2Type> droppableEnemies = TR2TypeUtilities.GetDropperEnemies(!Settings.ProtectMonks, Settings.UnconditionalChickens, Settings.IsRemastered);
            newTypes.Add(SelectRequiredEnemy(droppableEnemies, levelName, difficulty));
        }

        if (!Settings.ReplaceRequiredEnemies || levelName == TR2LevelNames.HOME)
        {
            foreach (TR2Type type in TR2EnemyUtilities.GetRequiredEnemies(levelName))
            {
                if (!newTypes.Contains(type))
                {
                    newTypes.Add(type);
                }
            }
        }

        // Some secrets may have locked enemies in place - we must retain those types
        foreach (int itemIndex in ItemFactory.GetLockedItems(levelName))
        {
            TR2Entity item = level.Entities[itemIndex];
            if (TR2TypeUtilities.IsEnemyType(item.TypeID))
            {
                List<TR2Type> family = TR2TypeUtilities.GetFamily(TR2TypeUtilities.GetAliasForLevel(levelName, item.TypeID));
                if (!newTypes.Any(family.Contains))
                {
                    newTypes.Add(family[Generator.Next(0, family.Count)]);
                }
            }
        }

        // Get all other candidate supported enemies
        List<TR2Type> allEnemies = TR2TypeUtilities.GetCandidateCrossLevelEnemies(Settings.IsRemastered)
            .FindAll(e => TR2EnemyUtilities.IsEnemySupported(levelName, e, difficulty, Settings.ProtectMonks, Settings.IsRemastered));

        if (Settings.OneEnemyMode
            || (Settings.UseEnemyExclusions && Settings.IncludedEnemies.Count < newTypes.Capacity)
            || Settings.DragonSpawnType == DragonSpawnType.Minimum
            || !DragonLevels.Contains(levelName))
        {
            allEnemies.Remove(TR2Type.MarcoBartoli);
        }

        // Remove all exclusions from the pool, and adjust the target capacity
        allEnemies.RemoveAll(_excludedEnemies.Contains);

        IEnumerable<TR2Type> ex = allEnemies.Where(e => !newTypes.Any(TR2TypeUtilities.GetFamily(e).Contains));
        List<TR2Type> unalisedTypes = TR2TypeUtilities.RemoveAliases(ex);
        while (unalisedTypes.Count < newTypes.Capacity - newTypes.Count)
        {
            --newTypes.Capacity;
        }

        // Fill the remainder to capacity as randomly as we can
        HashSet<TR2Type> testedTypes = new();
        while (newTypes.Count < newTypes.Capacity && testedTypes.Count < allEnemies.Count)
        {
            TR2Type type;
            if (Settings.DragonSpawnType == DragonSpawnType.Maximum
                && !newTypes.Contains(TR2Type.MarcoBartoli)
                && TR2EnemyUtilities.IsEnemySupported(levelName, TR2Type.MarcoBartoli, difficulty, Settings.ProtectMonks, Settings.IsRemastered))
            {
                type = TR2Type.MarcoBartoli;
            }
            else
            {
                type = allEnemies[Generator.Next(0, allEnemies.Count)];
            }

            testedTypes.Add(type);

            // Check if the use of this enemy triggers an overwrite of the pool, for example
            // the dragon in HSH. Null means nothing special has been defined.
            var restrictedCombinations = TR2EnemyUtilities.GetPermittedCombinations(levelName, type, difficulty, Settings.IsRemastered);
            if (restrictedCombinations != null)
            {
                do
                {
                    // Pick a combination, ensuring we honour docile bird monsters if present,
                    // and try to select a group that doesn't contain an excluded enemy.
                    newTypes.Clear();
                    newTypes.AddRange(restrictedCombinations[Generator.Next(0, restrictedCombinations.Count)]);
                }
                while (Settings.DocileChickens && newTypes.Any(TR2TypeUtilities.IsBirdMonsterType) && chickenGuisers.All(g => newTypes.Contains(g))
                    || (newTypes.Any(_excludedEnemies.Contains) && restrictedCombinations.Any(c => !c.Any(_excludedEnemies.Contains))));
                break;
            }

            // If it's the chicken in HSH with default behaviour, we don't want it ending the level
            if (Settings.DefaultChickens && TR2TypeUtilities.IsBirdMonsterType(type)
                && levelName == TR2LevelNames.HOME && allEnemies.Except(newTypes).Count() > 1)
            {
                continue;
            }

            // If this is a tracked enemy throughout the game, we only allow it if the number
            // of unique levels is within the limit. Bear in mind we are collecting more than
            // one group of enemies per level.
            var actualType = TR2TypeUtilities.TranslateAlias(type);
            if (_gameEnemyTracker.TryGetValue(actualType, out var levels) && !levels.Contains(levelName))
            {
                if (levels.Count < levels.Capacity)
                {
                    levels.Add(levelName);
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

            List<TR2Type> family = TR2TypeUtilities.GetFamily(type);
            if (!newTypes.Any(family.Contains))
            {
                // #144 We can include docile chickens provided we aren't including everything
                // that can be disguised as a chicken.
                if (Settings.DocileChickens)
                {
                    bool guisersAvailable = !chickenGuisers.All(newTypes.Contains);
                    if (!guisersAvailable && TR2TypeUtilities.IsBirdMonsterType(type))
                    {
                        continue;
                    }

                    // If the selected type is a potential guiser, it can only be added if it's not
                    // the last available guiser. Otherwise, it will become the guiser.
                    if (chickenGuisers.Contains(type) && newTypes.Any(TR2TypeUtilities.IsBirdMonsterType))
                    {
                        if (newTypes.FindAll(chickenGuisers.Contains).Count == chickenGuisers.Count - 1)
                        {
                            continue;
                        }
                    }
                }

                newTypes.Add(type);
            }
        }

        // If everything we are including is restriced by room, we need to provide at least one other enemy type
        Dictionary<TR2Type, List<int>> restrictedRoomEnemies = TR2EnemyUtilities.GetRestrictedEnemyRooms(levelName, difficulty);
        if (restrictedRoomEnemies != null && newTypes.All(e => restrictedRoomEnemies.ContainsKey(e)))
        {
            List<TR2Type> pool = TR2TypeUtilities.GetDropperEnemies(!Settings.ProtectMonks, Settings.UnconditionalChickens, Settings.IsRemastered);
            do
            {
                TR2Type fallbackEnemy;
                do
                {
                    fallbackEnemy = pool[Generator.Next(0, pool.Count)];
                }
                while ((_excludedEnemies.Contains(fallbackEnemy) && pool.Any(e => !_excludedEnemies.Contains(e)))
                || newTypes.Contains(fallbackEnemy)
                || !TR2EnemyUtilities.IsEnemySupported(levelName, fallbackEnemy, difficulty, Settings.ProtectMonks, Settings.IsRemastered));
                newTypes.Add(fallbackEnemy);
            }
            while (newTypes.All(e => restrictedRoomEnemies.ContainsKey(e)));
        }
        else
        {
            // #345 Barkhang/Opera with only Winstons causes freezing issues
            List<TR2Type> friends = TR2TypeUtilities.GetAllies();
            if (_friendlyLimitLevels.Contains(levelName) && newTypes.All(friends.Contains))
            {
                // Add an additional "safe" enemy - so pick from the droppable range, monks and chickens excluded
                List<TR2Type> droppableEnemies = TR2TypeUtilities.GetDropperEnemies(false, false, Settings.IsRemastered);
                newTypes.Add(SelectRequiredEnemy(droppableEnemies, levelName, difficulty));
            }
        }

        // #144 Decide at this point who will be guising unless it has already been decided above (e.g. HSH)          
        if (Settings.DocileChickens && newTypes.Any(TR2TypeUtilities.IsBirdMonsterType) && chickenGuiser == TR2Type.BirdMonster)
        {
            int guiserIndex = chickenGuisers.FindIndex(g => !newTypes.Contains(g));
            if (guiserIndex != -1)
            {
                chickenGuiser = chickenGuisers[guiserIndex];
            }
        }

        return new()
        {
            TypesToImport = newTypes,
            TypesToRemove = oldTypes,
            BirdMonsterGuiser = chickenGuiser
        };
    }

    public static List<TR2Entity> GetEnemyEntities(TR2Level level)
    {
        List<TR2Type> allEnemies = TR2TypeUtilities.GetFullListOfEnemies();
        return level.Entities.FindAll(e => allEnemies.Contains(e.TypeID));
    }

    public static List<TR2Type> GetCurrentEnemyTypes(TR2Level level)
    {
        var allTypes = TR2TypeUtilities.GetFullListOfEnemies();
        return [.. level.Entities.Select(e => e.TypeID)
            .Where(allTypes.Contains)
            .Distinct()];
    }

    public EnemyRandomizationCollection<TR2Type> RandomizeEnemiesNatively(string levelName, TR2Level level)
    {
        if (levelName == TR2LevelNames.ASSAULT)
        {
            return null;
        }

        if (Settings.DocileChickens && levelName == TR2LevelNames.CHICKEN)
        {
            DisguiseType(levelName, level.Models, TR2Type.MaskedGoon1, TR2Type.BirdMonster);
        }

        var enemies = new EnemyRandomizationCollection<TR2Type>
        {
            BirdMonsterGuiser = TR2Type.MaskedGoon1,
        };

        if (Settings.IsRemastered || !Settings.UseEnemyClones || !Settings.CloneOriginalEnemies)
        {
            enemies.Available.AddRange(GetCurrentEnemyTypes(level));
            enemies.Water.AddRange(TR2TypeUtilities.FilterWaterEnemies(enemies.Available));
            enemies.Droppable.AddRange(TR2TypeUtilities.FilterDropperEnemies(enemies.Available,
                !Settings.ProtectMonks, Settings.UnconditionalChickens, Settings.IsRemastered));
        }

        RandomizeEnemies(levelName, level, enemies);

        return enemies;
    }

    public static void DisguiseType(string levelName, TRDictionary<TR2Type, TRModel> modelData, TR2Type guiser, TR2Type targetType)
    {
        guiser = TR2TypeUtilities.TranslateAlias(guiser);
        if (targetType == TR2Type.BirdMonster && levelName == TR2LevelNames.CHICKEN)
        {
            // We have to keep the original model for the boss, so in
            // this instance we just clone the model for the guiser
            modelData[guiser] = modelData[targetType].Clone();
        }
        else
        {
            modelData.ChangeKey(targetType, guiser);
        }
    }

    public void RandomizeEnemies(string levelName, TR2Level level, EnemyRandomizationCollection<TR2Type> enemies)
    {
        bool shotgunGoonSeen = levelName == TR2LevelNames.HOME;
        bool dragonSeen = levelName == TR2LevelNames.LAIR;

        List<TR2Entity> enemyEntities = GetEnemyEntities(level);
        RandoDifficulty difficulty = GetImpliedDifficulty();

        if (Settings.IsRemastered && levelName == TR2LevelNames.HOME && !enemies.Available.Contains(TR2Type.Doberman))
        {
            // The game requires 15 items of type dog, stick goon or masked goon. The models will have been
            // eliminated at this stage, so just create a placeholder to trigger the correct HSH behaviour.
            level.Models[TR2Type.Doberman] = new()
            {
                Meshes = [level.Models[TR2Type.Lara].Meshes.First()]
            };

            short angleDiff = (short)Math.Ceiling(ushort.MaxValue / (_hshPlaceholderCount + 1d));
            for (int i = 0; i < _hshPlaceholderCount; i++)
            {
                level.Entities.Add((TR2Entity)_hshPlaceholderDog.Clone());
                level.Entities[^1].Angle -= (short)((i + 1) * angleDiff);
            }
        }

        // First iterate through any enemies that are restricted by room
        Dictionary<TR2Type, List<int>> enemyRooms = TR2EnemyUtilities.GetRestrictedEnemyRooms(levelName, difficulty);
        if (enemyRooms != null)
        {
            foreach (TR2Type type in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(type))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[type];
                int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(type, difficulty);
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
                    TR2Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[type][Generator.Next(0, enemyRooms[type].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR2TypeUtilities.IsWaterCreature(type) && level.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    targetEntity.TypeID = TR2TypeUtilities.TranslateAlias(type);
                    SetOneShot(targetEntity, level.Entities.IndexOf(targetEntity), level.FloorData);
                    enemyEntities.Remove(targetEntity);
                }

                // Remove this entity type from the available rando pool
                enemies.Available.Remove(type);
            }
        }

        foreach (TR2Entity currentEntity in enemyEntities)
        {
            if (enemies.Available.Count == 0)
            {
                continue;
            }

            TR2Type currentType = currentEntity.TypeID;
            TR2Type newType = currentType;
            int enemyIndex = level.Entities.IndexOf(currentEntity);

            // If it's an existing enemy that has to remain in the same spot, skip it
            if ((!Settings.ReplaceRequiredEnemies || levelName == TR2LevelNames.HOME)
                && TR2EnemyUtilities.IsEnemyRequired(levelName, currentType))
            {
                continue;
            }

            if (ItemFactory.IsItemLocked(levelName, enemyIndex))
            {
                continue;
            }

            // Generate a new type, ensuring to test for item drops
            newType = enemies.Available[Generator.Next(0, enemies.Available.Count)];
            bool hasPickupItem = level.Entities
                .Any(item => TR2EnemyUtilities.HasDropItem(currentEntity, item));

            if (hasPickupItem
                && !TR2TypeUtilities.CanDropPickups(newType, !Settings.ProtectMonks, Settings.UnconditionalChickens, Settings.IsRemastered))
            {
                newType = enemies.Droppable[Generator.Next(0, enemies.Droppable.Count)];
            }

            short roomIndex = currentEntity.Room;
            TR2Room room = level.Rooms[roomIndex];

            if (levelName == TR2LevelNames.DA && roomIndex == _platformEndRoom)
            {
                // Make sure the end level trigger isn't blocked by an unkillable enemy
                while (TR2TypeUtilities.IsHazardCreature(newType) || (Settings.ProtectMonks && TR2TypeUtilities.IsMonk(newType)))
                {
                    newType = enemies.Available[Generator.Next(0, enemies.Available.Count)];
                }
            }

            if (TR2TypeUtilities.IsWaterCreature(currentType) && !TR2TypeUtilities.IsWaterCreature(newType))
            {
                // Check alternate rooms too - e.g. rooms 74/48 in 40 Fathoms
                short roomDrainIndex = -1;
                if (room.ContainsWater)
                {
                    roomDrainIndex = roomIndex;
                }
                else if (room.AlternateRoom != -1 && level.Rooms[room.AlternateRoom].ContainsWater)
                {
                    roomDrainIndex = room.AlternateRoom;
                }

                if (roomDrainIndex != -1)
                {
                    newType = enemies.Water[Generator.Next(0, enemies.Water.Count)];
                }
            }

            // Ensure that if we have to pick a different enemy at this point that we still
            // honour any pickups in the same spot.
            List<TR2Type> enemyPool = hasPickupItem ? enemies.Droppable : enemies.Available;

            while (newType == TR2Type.ShotgunGoon && shotgunGoonSeen) // HSH only
            {
                newType = enemyPool[Generator.Next(0, enemyPool.Count)];
            }

            while (newType == TR2Type.MarcoBartoli && dragonSeen) // DL only, other levels use quasi-zoning for the dragon
            {
                newType = enemyPool[Generator.Next(0, enemyPool.Count)];
            }

            // #278 Flamethrowers in room 29 after pulling the lever are too difficult, but if difficulty is set to unrestricted
            // and they do end up here, environment mods will change their positions.
            int totalRestrictionCount = TR2EnemyUtilities.GetRestrictedEnemyTotalTypeCount(difficulty);
            if (levelName == TR2LevelNames.FLOATER
                && difficulty == RandoDifficulty.Default
                && _floaterFlameEnemies.Contains(enemyIndex)
                && enemyPool.Count > totalRestrictionCount)
            {
                while (newType == TR2Type.FlamethrowerGoon)
                {
                    newType = enemyPool[Generator.Next(0, enemyPool.Count)];
                }
            }

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Winston).
            int maxEntityCount = TR2EnemyUtilities.GetRestrictedEnemyLevelCount(newType, difficulty);
            if (maxEntityCount != -1)
            {
                if (level.Entities.FindAll(e => e.TypeID == newType).Count >= maxEntityCount
                    && enemyPool.Count > totalRestrictionCount)
                {
                    TR2Type tmp = newType;
                    while (newType == tmp)
                    {
                        newType = enemyPool[Generator.Next(0, enemyPool.Count)];
                    }
                }
            }

            // Final step is to convert/set the type and ensure OneShot is set if needed (#146)
            if (Settings.DocileChickens && TR2TypeUtilities.IsBirdMonsterType(newType))
            {
                newType = enemies.BirdMonsterGuiser;
            }

            currentEntity.TypeID = TR2TypeUtilities.TranslateAlias(newType);
            SetOneShot(currentEntity, enemyIndex, level.FloorData);
            _resultantEnemies.Add(newType);
        }

        // MercSnowMobDriver relies on RedSnowmobile so it will be available in the model list
        if (!level.Entities.Any(e => e.TypeID == TR2Type.RedSnowmobile)
            && level.Entities.Find(e => e.TypeID == TR2Type.MercSnowmobDriver) is TR2Entity mercDriver)
        {
            TR2Entity skidoo = new()
            {
                TypeID = TR2Type.RedSnowmobile,
                Intensity1 = -1,
                Intensity2 = -1
            };
            level.Entities.Add(skidoo);

            Location randomLocation = VehicleUtilities.GetRandomLocation(levelName, level, TR2Type.RedSnowmobile, Generator)
                ?? mercDriver.GetLocation();
            skidoo.SetLocation(randomLocation);
        }

        // Check in case there are too many skidoo drivers
        if (level.Entities.Any(e => e.TypeID == TR2Type.MercSnowmobDriver))
        {
            LimitSkidooEntities(levelName, level);
        }

        // Or too many friends - #345
        List<TR2Type> friends = TR2TypeUtilities.GetAllies();
        if (_friendlyLimitLevels.Contains(levelName) && enemies.Available.Any(friends.Contains))
        {
            LimitFriendlyEnemies(level, enemies.Available.Except(friends).ToList(), friends);
        }

        RelocateEnemies(levelName, level.Entities);

        if (!Settings.AllowEnemyKeyDrops)
        {
            // Referenced here in case item randomization is not enabled.
            TR2ItemAllocator allocator = new()
            {
                Settings = Settings,
            };
            allocator.ExcludeEnemyKeyDrops(level.Entities);
        }
    }

    private void LimitSkidooEntities(string levelName, TR2Level level)
    {
        if (!Settings.IsRemastered)
        {
            return;
        }

        // Ensure that the total implied enemy count does not exceed that of the original
        // level. The limit actually varies depending on the number of traps and other objects
        // so for those levels with high entity counts, we further restrict the limit.
        int skidooLimit = TR2EnemyUtilities.GetSkidooDriverLimit(levelName);

        List<TR2Entity> enemies = GetEnemyEntities(level);
        int normalEnemyCount = enemies.FindAll(e => e.TypeID != TR2Type.MercSnowmobDriver).Count;
        int skidooMenCount = enemies.Count - normalEnemyCount;
        int skidooRemovalCount = skidooMenCount - skidooMenCount / 2;
        if (skidooLimit > 0)
        {
            while (skidooMenCount - skidooRemovalCount > skidooLimit)
            {
                ++skidooRemovalCount;
            }
        }

        if (skidooRemovalCount == 0)
        {
            return;
        }

        List<Location> pickupLocations = level.Entities
            .Where(e => TR2TypeUtilities.IsAnyPickupType(e.TypeID) && !TR2TypeUtilities.IsSecretType(e.TypeID))
            .Select(e => e.GetLocation())
            .ToList();

        List<TR2Type> replacementPool = !Settings.RandomizeItems || Settings.RandoItemDifficulty == ItemDifficulty.Default
            ? TR2TypeUtilities.GetAmmoTypes()
            : new() { TR2Type.CameraTarget_N };

        List<TR2Entity> skidMen;
        for (int i = 0; i < skidooRemovalCount; i++)
        {
            skidMen = level.Entities.FindAll(e => e.TypeID == TR2Type.MercSnowmobDriver);
            if (skidMen.Count == 0)
            {
                break;
            }

            // Select a random Skidoo driver and convert him into something else
            TR2Entity skidMan = skidMen[Generator.Next(0, skidMen.Count)];
            TR2Type newType = replacementPool[Generator.Next(0, replacementPool.Count)];
            skidMan.TypeID = newType;
            skidMan.Invisible = false;

            if (TR2TypeUtilities.IsAnyPickupType(newType))
            {
                skidMan.SetLocation(pickupLocations[Generator.Next(0, pickupLocations.Count)]);
            }

            level.FloorData.RemoveEntityTriggers(level.Entities.IndexOf(skidMan));
        }
    }

    private void LimitFriendlyEnemies(TR2Level level, List<TR2Type> pool, List<TR2Type> friends)
    {
        if (!Settings.IsRemastered)
        {
            return;
        }

        List<TR2Entity> levelFriends = level.Entities.FindAll(e => friends.Contains(e.TypeID));
        while (levelFriends.Count > _friendlyEnemyLimit)
        {
            TR2Entity entity = levelFriends[Generator.Next(0, levelFriends.Count)];
            entity.TypeID = TR2TypeUtilities.TranslateAlias(pool[Generator.Next(0, pool.Count)]);
            levelFriends.Remove(entity);
        }
    }
}
