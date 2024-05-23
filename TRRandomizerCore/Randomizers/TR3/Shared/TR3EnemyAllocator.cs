using Newtonsoft.Json;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR3EnemyAllocator : EnemyAllocator<TR3Type>
{
    private const int _willardSequence = 19;

    private readonly Dictionary<string, List<Location>> _pistolLocations;

    public ItemFactory<TR3Entity> ItemFactory { get; set; }

    public TR3EnemyAllocator()
    {
        _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\TR3\Locations\unarmed_locations.json"));
    }

    protected override Dictionary<TR3Type, List<string>> GetGameTracker()
        => TR3EnemyUtilities.PrepareEnemyGameTracker(Settings.RandoEnemyDifficulty);

    public EnemyTransportCollection<TR3Type> SelectCrossLevelEnemies(string levelName, TR3Level level, int levelSequence)
    {
        if (levelName == TR3LevelNames.ASSAULT)
        {
            return null;
        }

        List<TR3Type> oldTypes = GetCurrentEnemyEntities(level);
        List<TR3Type> allEnemies = TR3TypeUtilities.GetCandidateCrossLevelEnemies()
            .FindAll(e => TR3EnemyUtilities.IsEnemySupported(levelName, e, Settings.RandoEnemyDifficulty));

        int enemyCount = oldTypes.Count + TR3EnemyUtilities.GetEnemyAdjustmentCount(levelName);
        List<TR3Type> newTypes = new(enemyCount);

        if (TR3TypeUtilities.GetWaterEnemies().Any(oldTypes.Contains))
        {
            List<TR3Type> waterEnemies = TR3TypeUtilities.GetKillableWaterEnemies();
            newTypes.Add(SelectRequiredEnemy(waterEnemies, levelName, Settings.RandoEnemyDifficulty));
        }

        bool droppableEnemyRequired = TR3EnemyUtilities.IsDroppableEnemyRequired(level);
        if (droppableEnemyRequired)
        {
            List<TR3Type> droppableEnemies = TR3TypeUtilities.FilterDroppableEnemies(allEnemies, Settings.ProtectMonks);
            newTypes.Add(SelectRequiredEnemy(droppableEnemies, levelName, Settings.RandoEnemyDifficulty));
        }

        foreach (TR3Type type in TR3EnemyUtilities.GetRequiredEnemies(levelName))
        {
            if (!newTypes.Contains(type))
            {
                newTypes.Add(type);
            }
        }

        foreach (int itemIndex in ItemFactory.GetLockedItems(levelName))
        {
            TR3Entity item = level.Entities[itemIndex];
            if (TR3TypeUtilities.IsEnemyType(item.TypeID))
            {
                List<TR3Type> family = TR3TypeUtilities.GetFamily(TR3TypeUtilities.GetAliasForLevel(levelName, item.TypeID));
                if (!newTypes.Any(family.Contains))
                {
                    newTypes.Add(family[Generator.Next(0, family.Count)]);
                }
            }
        }

        if (!Settings.DocileWillard || Settings.OneEnemyMode || Settings.IncludedEnemies.Count < newTypes.Capacity)
        {
            // Willie isn't excludable in his own right because supporting a Willie-only game is impossible
            allEnemies.Remove(TR3Type.Willie);
        }

        // Remove all exclusions from the pool, and adjust the target capacity
        allEnemies.RemoveAll(_excludedEnemies.Contains);

        IEnumerable<TR3Type> ex = allEnemies.Where(e => !newTypes.Any(TR3TypeUtilities.GetFamily(e).Contains));
        List<TR3Type> unalisedTypes = TR3TypeUtilities.RemoveAliases(ex);
        while (unalisedTypes.Count < newTypes.Capacity - newTypes.Count)
        {
            --newTypes.Capacity;
        }

        // Fill the list from the remaining candidates. Keep track of ones tested to avoid
        // looping infinitely if it's not possible to fill to capacity
        HashSet<TR3Type> testedTypes = new();
        while (newTypes.Count < newTypes.Capacity && testedTypes.Count < allEnemies.Count)
        {
            TR3Type type = allEnemies[Generator.Next(0, allEnemies.Count)];
            testedTypes.Add(type);

            if (!TR3EnemyUtilities.IsEnemySupported(levelName, type, Settings.RandoEnemyDifficulty))
            {
                continue;
            }

            if (type == TR3Type.Willie && levelName == TR3LevelNames.WILLIE && levelSequence != _willardSequence)
            {
                continue;
            }

            // Monkeys are friendly when the tiger model is present, and when they are friendly,
            // mounting a vehicle will crash the game.
            if (level.Entities.Any(e => TR3TypeUtilities.IsVehicleType(e.TypeID))
                && ((type == TR3Type.Monkey && newTypes.Contains(TR3Type.Tiger))
                || (type == TR3Type.Tiger && newTypes.Contains(TR3Type.Monkey))))
            {
                continue;
            }

            if (_gameEnemyTracker.ContainsKey(type) && !_gameEnemyTracker[type].Contains(levelName))
            {
                if (_gameEnemyTracker[type].Count < _gameEnemyTracker[type].Capacity)
                {
                    _gameEnemyTracker[type].Add(levelName);
                }
                else
                {
                    if (allEnemies.Except(newTypes).Count() > 1)
                    {
                        continue;
                    }
                }
            }

            List<TR3Type> family = TR3TypeUtilities.GetFamily(type);
            if (!newTypes.Any(family.Contains))
            {
                newTypes.Add(type);
            }
        }

        if (newTypes.Count == 0
            || (newTypes.Capacity > 1 && newTypes.All(e => TR3EnemyUtilities.IsEnemyRestricted(levelName, e))))
        {
            // Make sure we have an unrestricted enemy available for the individual level conditions. This will
            // guarantee a "safe" enemy for the level; we avoid aliases here to avoid further complication.
            bool RestrictionCheck(TR3Type e) =>
                (droppableEnemyRequired && !TR3TypeUtilities.CanDropPickups(e, Settings.ProtectMonks))
                || !TR3EnemyUtilities.IsEnemySupported(levelName, e, Settings.RandoEnemyDifficulty)
                || newTypes.Contains(e)
                || TR3TypeUtilities.IsWaterCreature(e)
                || TR3EnemyUtilities.IsEnemyRestricted(levelName, e)
                || TR3TypeUtilities.TranslateAlias(e) != e;

            List<TR3Type> unrestrictedPool = allEnemies.FindAll(e => !RestrictionCheck(e));
            if (unrestrictedPool.Count == 0)
            {
                // We are going to have to pull in the full list of candidates again, so ignoring any user-defined exclusions
                unrestrictedPool = TR3TypeUtilities.GetCandidateCrossLevelEnemies().FindAll(e => !RestrictionCheck(e));
            }

            newTypes.Add(unrestrictedPool[Generator.Next(0, unrestrictedPool.Count)]);
        }

        return new()
        {
            TypesToImport = newTypes,
            TypesToRemove = oldTypes
        };
    }

    private static List<TR3Type> GetCurrentEnemyEntities(TR3Level level)
    {
        List<TR3Type> allGameEnemies = TR3TypeUtilities.GetFullListOfEnemies();
        SortedSet<TR3Type> allLevelEnts = new(level.Entities.Select(e => e.TypeID));
        return allLevelEnts.Where(allGameEnemies.Contains).ToList();
    }

    private TR3Type SelectRequiredEnemy(List<TR3Type> pool, string levelName, RandoDifficulty difficulty)
    {
        pool.RemoveAll(e => !TR3EnemyUtilities.IsEnemySupported(levelName, e, difficulty));

        TR3Type type;
        if (pool.All(_excludedEnemies.Contains))
        {
            // Select the last excluded enemy (lowest priority)
            type = _excludedEnemies.Last(pool.Contains);
        }
        else
        {
            do
            {
                type = pool[Generator.Next(0, pool.Count)];
            }
            while (_excludedEnemies.Contains(type));
        }

        return type;
    }

    public EnemyRandomizationCollection<TR3Type> RandomizeEnemiesNatively(string levelName, TR3Level level, int levelSequence)
    {
        if (levelName == TR3LevelNames.ASSAULT)
        {
            return null;
        }

        List<TR3Type> availableEnemyTypes = GetCurrentEnemyEntities(level);
        if (level.Entities.Any(e => TR3TypeUtilities.IsVehicleType(e.TypeID))
            && availableEnemyTypes.Contains(TR3Type.Tiger)
            && availableEnemyTypes.Contains(TR3Type.Monkey))
        {
            TR3Type banishedType = Generator.NextDouble() < 0.5 ? TR3Type.Tiger : TR3Type.Monkey;
            availableEnemyTypes.Remove(banishedType);
            level.Models.Remove(banishedType);
        }

        EnemyRandomizationCollection<TR3Type> enemies = new()
        {
            Available = availableEnemyTypes,
            Droppable = TR3TypeUtilities.FilterDroppableEnemies(availableEnemyTypes, Settings.ProtectMonks),
            Water = TR3TypeUtilities.FilterWaterEnemies(availableEnemyTypes)
        };

        RandomizeEnemies(levelName, level, levelSequence, enemies);

        return enemies;
    }

    public void RandomizeEnemies(string levelName, TR3Level level, int levelSequence, EnemyRandomizationCollection<TR3Type> enemies)
    {
        List<TR3Type> allEnemies = TR3TypeUtilities.GetFullListOfEnemies();
        List<TR3Entity> enemyEntities = level.Entities.FindAll(e => allEnemies.Contains(e.TypeID));

        // First iterate through any enemies that are restricted by room
        Dictionary<TR3Type, List<int>> enemyRooms = TR3EnemyUtilities.GetRestrictedEnemyRooms(levelName, Settings.RandoEnemyDifficulty);
        if (enemyRooms != null)
        {
            foreach (TR3Type type in enemyRooms.Keys)
            {
                if (!enemies.Available.Contains(type))
                {
                    continue;
                }

                List<int> rooms = enemyRooms[type];
                int maxEntityCount = TR3EnemyUtilities.GetRestrictedEnemyLevelCount(type, Settings.RandoEnemyDifficulty);
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
                    TR3Entity targetEntity = null;
                    do
                    {
                        int room = enemyRooms[type][Generator.Next(0, enemyRooms[type].Count)];
                        targetEntity = enemyEntities.Find(e => e.Room == room);
                    }
                    while (targetEntity == null);

                    // If the room has water but this enemy isn't a water enemy, we will assume that environment
                    // modifications will handle assignment of the enemy to entities.
                    if (!TR3TypeUtilities.IsWaterCreature(type) && level.Rooms[targetEntity.Room].ContainsWater)
                    {
                        continue;
                    }

                    // Some enemies need pathing like Willard but we have to honour the entity limit
                    List<Location> paths = TR3EnemyUtilities.GetAIPathing(levelName, type, targetEntity.Room);
                    if (ItemFactory.CanCreateItems(levelName, level.Entities, paths.Count))
                    {
                        targetEntity.TypeID = TR3TypeUtilities.TranslateAlias(type);
                        TR3EnemyUtilities.SetEntityTriggers(level, targetEntity);
                        enemyEntities.Remove(targetEntity);

                        // Add the pathing if necessary
                        foreach (Location path in paths)
                        {
                            TR3Entity pathItem = ItemFactory.CreateItem(levelName, level.Entities, path);
                            pathItem.TypeID = TR3Type.AIPath_N;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                enemies.Available.Remove(type);
            }
        }

        foreach (TR3Entity currentEntity in enemyEntities)
        {
            TR3Type currentType = currentEntity.TypeID;
            TR3Type newType = currentType;
            int enemyIndex = level.Entities.IndexOf(currentEntity);

            // If it's an existing enemy that has to remain in the same spot, skip it
            if (TR3EnemyUtilities.IsEnemyRequired(levelName, currentType)
                || ItemFactory.IsItemLocked(levelName, enemyIndex))
            {
                continue;
            }

            List<TR3Type> enemyPool = enemies.Available;
            if (level.Entities.Any(item => TR3EnemyUtilities.HasDropItem(currentEntity, item)))
            {
                enemyPool = enemies.Droppable;
            }
            else if (TR3TypeUtilities.IsWaterCreature(currentType))
            {
                enemyPool = enemies.Water;
            }

            newType = enemyPool[Generator.Next(0, enemyPool.Count)];

            // If we are restricting count per level for this enemy and have reached that count, pick
            // something else. This applies when we are restricting by in-level count, but not by room
            // (e.g. Winston).
            int maxEntityCount = TR3EnemyUtilities.GetRestrictedEnemyLevelCount(newType, Settings.RandoEnemyDifficulty);
            if (maxEntityCount != -1)
            {
                if (level.Entities.FindAll(e => e.TypeID == newType).Count >= maxEntityCount && enemyPool.Count > maxEntityCount)
                {
                    TR3Type tmp = newType;
                    while (newType == tmp || TR3EnemyUtilities.IsEnemyRestricted(levelName, newType))
                    {
                        newType = enemyPool[Generator.Next(0, enemyPool.Count)];
                    }
                }
            }

            TR3Entity targetEntity = currentEntity;

            if (levelName == TR3LevelNames.CRASH && currentEntity.Room == 15)
            {
                // Crash site raptor spawns need special treatment. The 3 entities in this (unreachable) room
                // are normally raptors, and the game positions them to the spawn points. If we no longer have
                // raptors, then replace the spawn points with the actual enemies. Otherwise, ensure they remain
                // as raptors.
                if (!enemies.Available.Contains(TR3Type.Raptor))
                {
                    TR3Entity raptorSpawn = level.Entities.Find(e => e.TypeID == TR3Type.RaptorRespawnPoint_N && e.Room != 15);
                    if (raptorSpawn != null)
                    {
                        (targetEntity = raptorSpawn).TypeID = TR3TypeUtilities.TranslateAlias(newType);
                        currentEntity.TypeID = TR3Type.RaptorRespawnPoint_N;
                    }
                }
            }
            else if (levelName == TR3LevelNames.RXTECH
                && levelSequence == _willardSequence
                && Settings.RandoEnemyDifficulty == RandoDifficulty.Default
                && newType == TR3Type.RXTechFlameLad
                && (currentEntity.Room == 14 || currentEntity.Room == 45))
            {
                // #269 We don't want flamethrowers here because they're hostile, so getting off the minecart
                // safely is too difficult. We can only change them if there is something else unrestricted available.
                List<TR3Type> safePool = enemyPool.FindAll(e => e != TR3Type.RXTechFlameLad && !TR3EnemyUtilities.IsEnemyRestricted(levelName, e));
                if (safePool.Count > 0)
                {
                    newType = safePool[Generator.Next(0, safePool.Count)];
                }
            }
            else if (levelName == TR3LevelNames.HSC)
            {
                if (currentEntity.Room == 87 && newType != TR3Type.Prisoner)
                {
                    // #271 The prisoner is needed here to activate the heavy trigger for the trapdoor. If we still have
                    // prisoners in the pool, ensure one is chosen. If this isn't the case, environment rando will provide
                    // a workaround.
                    if (enemies.Available.Contains(TR3Type.Prisoner))
                    {
                        newType = TR3Type.Prisoner;
                    }
                }
                else if (currentEntity.Room == 78 && newType == TR3Type.Monkey)
                {
                    // #286 Monkeys cannot share AI Ambush spots largely, but these are needed here to ensure the enemies
                    // come through the gate before the timer closes them again. Just ensure no monkeys are here.
                    List<TR3Type> safePool = enemyPool.FindAll(e => e != TR3Type.Monkey && !TR3EnemyUtilities.IsEnemyRestricted(levelName, e));
                    if (safePool.Count > 0)
                    {
                        newType = safePool[Generator.Next(0, safePool.Count)];
                    }
                    else
                    {
                        // Full monkey mode means we have to move them inside the gate
                        currentEntity.Z -= 4096;
                    }
                }
            }
            else if (levelName == TR3LevelNames.THAMES && (currentEntity.Room == 61 || currentEntity.Room == 62) && newType == TR3Type.Monkey)
            {
                // #286 Move the monkeys away from the AI entities
                currentEntity.Z -= TRConsts.Step4;
            }

            if (targetEntity.TypeID == TR3Type.Cobra)
            {
                targetEntity.Invisible = false;
            }

            // Final step is to convert/set the type and ensure OneShot is set if needed (#146)
            targetEntity.TypeID = TR3TypeUtilities.TranslateAlias(newType);
            TR3EnemyUtilities.SetEntityTriggers(level, targetEntity);
            _resultantEnemies.Add(newType);
        }

        if (!Settings.AllowEnemyKeyDrops && (!Settings.RandomizeItems || !Settings.IncludeKeyItems))
        {
            // Shift enemies who are on top of key items so they don't pick them up.
            IEnumerable<TR3Entity> keyEnemies = level.Entities.Where(enemy => TR3TypeUtilities.IsEnemyType(enemy.TypeID)
                  && level.Entities.Any(key => TR3TypeUtilities.IsKeyItemType(key.TypeID)
                  && key.GetLocation().IsEquivalent(enemy.GetLocation()))
            );

            foreach (TR3Entity enemy in keyEnemies)
            {
                enemy.X++;
            }
        }
    }

    public List<Location> GetPistolLocations(string levelName)
        => _pistolLocations[levelName];

    public void AddUnarmedLevelAmmo(string levelName, TR3Level level, Action<Location, TR3Type> createItemCallback)
    {
        if (!Settings.CrossLevelEnemies || !Settings.GiveUnarmedItems)
        {
            return;
        }

        List<TR3Type> weaponTypes = TR3TypeUtilities.GetWeaponPickups();
        TR3Entity weaponEntity = level.Entities.Find(e =>
            weaponTypes.Contains(e.TypeID)
            && _pistolLocations[levelName].Any(l => l.IsEquivalent(e.GetLocation())));

        if (weaponEntity == null)
        {
            return;
        }

        Location weaponLocation = weaponEntity.GetLocation();

        List<TR3Type> allEnemies = TR3TypeUtilities.GetFullListOfEnemies();
        List<TR3Entity> levelEnemies = level.Entities.FindAll(e => allEnemies.Contains(e.TypeID));
        EnemyDifficulty difficulty = TR3EnemyUtilities.GetEnemyDifficulty(levelEnemies);

        if (difficulty > EnemyDifficulty.Easy)
        {
            while (weaponEntity.TypeID == TR3Type.Pistols_P)
            {
                weaponEntity.TypeID = weaponTypes[Generator.Next(0, weaponTypes.Count)];
            }
        }

        if (difficulty > EnemyDifficulty.Medium
            && !level.Entities.Any(e => e.TypeID == TR3Type.Pistols_P))
        {
            createItemCallback(weaponLocation, TR3Type.Pistols_P);
        }

        int ammoAllocation = TR3EnemyUtilities.GetStartingAmmo(weaponEntity.TypeID);
        if (ammoAllocation > 0)
        {
            ammoAllocation *= (int)difficulty;
            TR3Type ammoType = TR3TypeUtilities.GetWeaponAmmo(weaponEntity.TypeID);
            for (int i = 0; i < ammoAllocation; i++)
            {
                createItemCallback(weaponLocation, ammoType);
            }
        }

        if (difficulty == EnemyDifficulty.Medium || difficulty == EnemyDifficulty.Hard)
        {
            createItemCallback(weaponLocation, TR3Type.SmallMed_P);
            createItemCallback(weaponLocation, TR3Type.LargeMed_P);
        }
        if (difficulty > EnemyDifficulty.Medium)
        {
            createItemCallback(weaponLocation, TR3Type.LargeMed_P);
        }
        if (difficulty == EnemyDifficulty.VeryHard)
        {
            createItemCallback(weaponLocation, TR3Type.LargeMed_P);
        }
    }
}
