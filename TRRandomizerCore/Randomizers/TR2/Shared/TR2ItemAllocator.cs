using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2ItemAllocator : ItemAllocator<TR2Type, TR2Entity>
{
    public short DefaultItemShade { get; set; }

    public TR2ItemAllocator()
        : base(TRGameVersion.TR2) { }

    public override List<int> GetExcludedItems(string levelName)
        => new();

    protected override TR2Type GetPistolType()
        => TR2Type.Pistols_S_P;

    protected override List<TR2Type> GetStandardItemTypes()
        => TR2TypeUtilities.GetStandardPickupTypes();

    protected override List<TR2Type> GetWeaponItemTypes()
        => TR2TypeUtilities.GetGunTypes();

    protected override List<TR2Type> GetKeyItemTypes()
        => TR2TypeUtilities.GetKeyItemTypes();

    protected override List<TR2Type> GetEnemyTypes()
        => TR2TypeUtilities.GetFullListOfEnemies();

    protected override bool IsCrystalPickup(TR2Type type)
        => false;

    protected override void ItemMoved(TR2Entity item)
        => item.Intensity1 = item.Intensity2 = DefaultItemShade;

    public void RandomizeItems(string levelName, TR2Level level, AbstractTRScriptedLevel scriptedLevel)
    {
        if (levelName == TR2LevelNames.HOME && (scriptedLevel.RemovesWeapons || scriptedLevel.RemovesAmmo))
        {
            return;
        }

        InitialisePicker(levelName, level, Settings.ItemMode == ItemMode.Default ? LocationMode.Default : LocationMode.ExistingItems);

        if (Settings.ItemMode == ItemMode.Default)
        {
            if (levelName != TR2LevelNames.ASSAULT)
            {
                RandomizeItemTypes(levelName, level.Entities, scriptedLevel.RemovesWeapons);
            }
            RandomizeItemLocations(levelName, level.Entities, scriptedLevel.RemovesWeapons);
            EnforceOneLimit(levelName, level.Entities, scriptedLevel.RemovesWeapons);
        }
        else
        {
            ShuffleItems(levelName, level.Entities, scriptedLevel.RemovesWeapons, scriptedLevel.OriginalSequence,
                e => LocationUtilities.HasPickupTriger(e, level.Entities.IndexOf(e), level));
        }
    }

    public void RandomizeKeyItems(string levelName, TR2Level level, int originalSequence)
    {
        InitialisePicker(levelName, level, LocationMode.KeyItems);

        for (int i = 0; i < level.Entities.Count; i++)
        {
            TR2Entity entity = level.Entities[i];
            if (!TR2TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            bool hasPickupTrigger = LocationUtilities.HasPickupTriger(entity, i, level);
            _picker.RandomizeKeyItemLocation(entity, hasPickupTrigger, originalSequence);
            ItemMoved(entity);
        }
    }

    private void InitialisePicker(string levelName, TR2Level level, LocationMode locationMode)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level);
        _picker.KeyItemTestAction = (location, hasPickupTrigger, roomPool)
            => TestKeyItemLocation(location, hasPickupTrigger, roomPool, levelName, level);
        _picker.RoomInfos = new(level.Rooms.Select(r => new ExtRoomInfo(r)));

        List<Location> pool = GetItemLocationPool(levelName, level, locationMode != LocationMode.Default);
        if (locationMode == LocationMode.ExistingItems)
        {
            IEnumerable<Location> itemLocations = GetPickups(levelName, level.Entities, true)
                .Select(e => e.GetLocation())
                .DistinctBy(l => level.GetRoomSector(l));
            pool = new(itemLocations.Where(i => pool.Any(e => level.GetRoomSector(i) == level.GetRoomSector(e))));
        }
        _picker.Initialise(levelName, pool, Settings, Generator);
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, List<short> roomPool, string levelName, TR2Level level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the enemy can drop the item. Ensure too that the enemy
        // can be triggered from within the key item's room pool and not beyond.
        TR2Entity enemy = level.Entities
            .FindAll(e => TR2TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR2TypeUtilities.CanDropPickups
        (
            TR2TypeUtilities.GetAliasForLevel(levelName, enemy.TypeID),
            Settings.RandomizeEnemies && !Settings.ProtectMonks,
            Settings.RandomizeEnemies && Settings.UnconditionalChickens
        )
        && level.FloorData.GetTriggerRooms(level.Entities.IndexOf(enemy), level.Rooms).Any(roomPool.Contains));
    }

    private List<Location> GetItemLocationPool(string levelName, TR2Level level, bool keyItemMode)
    {
        List<Location> exclusions = new();
        if (_excludedLocations.ContainsKey(levelName))
        {
            exclusions.AddRange(_excludedLocations[levelName]);
        }

        exclusions.AddRange(level.Entities
            .Where(e => !TR2TypeUtilities.CanSharePickupSpace(e.TypeID))
            .Select(e => e.GetFloorLocation(loc => level.GetRoomSector(loc))));

        TR2LocationGenerator generator = new();
        return generator.Generate(level, exclusions, keyItemMode);
    }
}
