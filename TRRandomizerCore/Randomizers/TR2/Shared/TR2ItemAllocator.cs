using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR2ItemAllocator : ItemAllocator<TR2Type, TR2Entity>
{
    public TR2ItemAllocator()
        : base(TRGameVersion.TR2) { }

    protected override List<int> GetExcludedItems(string levelName)
        => new();

    protected override TR2Type GetPistolType()
        => TR2Type.Pistols_S_P;

    protected override List<TR2Type> GetStandardItemTypes()
        => TR2TypeUtilities.GetStandardPickupTypes();

    protected override List<TR2Type> GetWeaponItemTypes()
        => TR2TypeUtilities.GetGunTypes();

    protected override bool IsCrystalPickup(TR2Type type)
        => false;

    protected override void ItemMoved(TR2Entity item)
        => item.Intensity1 = item.Intensity2 = -1;

    public void RandomizeItems(string levelName, TR2Level level, AbstractTRScriptedLevel scriptedLevel)
    {
        if (levelName == TR2LevelNames.HOME && (scriptedLevel.RemovesWeapons || scriptedLevel.RemovesAmmo))
        {
            return;
        }

        _picker.Initialise(levelName, GetItemLocationPool(levelName, level, false), Settings, Generator);

        if (levelName != TR2LevelNames.ASSAULT)
        {
            RandomizeItemTypes(levelName, level.Entities, scriptedLevel.RemovesWeapons);
        }
        RandomizeItemLocations(levelName, level.Entities, scriptedLevel.RemovesWeapons);
        EnforceOneLimit(levelName, level.Entities, scriptedLevel.RemovesWeapons);
    }

    public void RandomizeKeyItems(string levelName, TR2Level level, int originalSequence)
    {
        _picker.TriggerTestAction = location => LocationUtilities.HasAnyTrigger(location, level);
        _picker.KeyItemTestAction = (location, hasPickupTrigger) => TestKeyItemLocation(location, hasPickupTrigger, levelName, level);
        _picker.RoomInfos = level.Rooms
            .Select(r => new ExtRoomInfo(r.Info, r.NumXSectors, r.NumZSectors))
            .ToList();

        _picker.Initialise(levelName, GetItemLocationPool(levelName, level, true), Settings, Generator);

        for (int i = 0; i < level.Entities.Count; i++)
        {
            TR2Entity entity = level.Entities[i];
            if (!TR2TypeUtilities.IsKeyItemType(entity.TypeID)
                || ItemFactory.IsItemLocked(levelName, i))
            {
                continue;
            }

            _picker.RandomizeKeyItemLocation(
                entity, LocationUtilities.HasPickupTriger(entity, i, level),
                originalSequence, level.Rooms[entity.Room].Info);
            ItemMoved(entity);
        }
    }

    private bool TestKeyItemLocation(Location location, bool hasPickupTrigger, string levelName, TR2Level level)
    {
        // Make sure if we're placing on the same tile as an enemy, that the enemy can drop the item.
        TR2Entity enemy = level.Entities
            .FindAll(e => TR2TypeUtilities.IsEnemyType(e.TypeID))
            .Find(e => e.GetLocation().IsEquivalent(location));

        return enemy == null || (Settings.AllowEnemyKeyDrops && !hasPickupTrigger && TR2TypeUtilities.CanDropPickups
        (
            TR2TypeUtilities.GetAliasForLevel(levelName, enemy.TypeID),
            Settings.RandomizeEnemies && !Settings.ProtectMonks,
            Settings.RandomizeEnemies && Settings.UnconditionalChickens
        ));
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
