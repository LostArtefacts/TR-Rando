using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("OriginalIO")]
public class FDTests : FDTestBase
{
    [TestMethod]
    [Description("Get a sector using sector units.")]
    public void GetSectorByVal()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(5, 9, TRUnit.Sector);

        Assert.AreEqual(5 * room.NumZSectors + 9, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using room units.")]
    public void GetSectorByRoomPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(5632, 9728, TRUnit.Room);

        Assert.AreEqual(5 * room.NumZSectors + 9, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using world units.")]
    public void GetSectorByWorldPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(9728, 18944, TRUnit.World);

        Assert.AreEqual(5 * room.NumZSectors + 9, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X-).")]
    public void GetSectorByOOBXNeg()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(512, 22016, TRUnit.World);

        Assert.AreEqual(0 * room.NumZSectors + 12, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X+).")]
    public void GetSectorByOOBXPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(11776, 22016, TRUnit.World);

        Assert.AreEqual(6 * room.NumZSectors + 12, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (Z+).")]
    public void GetSectorByOOBZPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(5632, 24064, TRUnit.World);

        Assert.AreEqual(1 * room.NumZSectors + 13, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (Z-).")]
    public void GetSectorByOOBZNeg()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(5632, 8704, TRUnit.World);

        Assert.AreEqual(1 * room.NumZSectors + 0, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X-Z+).")]
    public void GetSectorByOOBXNegZPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(512, 24064, TRUnit.World);

        Assert.AreEqual(0 * room.NumZSectors + 13, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X+Z+).")]
    public void GetSectorByOOBXPosZPos()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(11776, 24064, TRUnit.World);

        Assert.AreEqual(6 * room.NumZSectors + 13, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X-Z-).")]
    public void GetSectorByOOBXNegZNeg()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(512, 8704, TRUnit.World);

        Assert.AreEqual(0 * room.NumZSectors + 0, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get a sector using OOB world units (X+Z-).")]
    public void GetSectorByOOBXPosZNeg()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(11776, 8704, TRUnit.World);

        Assert.AreEqual(6 * room.NumZSectors + 0, room.Sectors.IndexOf(sector));
    }

    [TestMethod]
    [Description("Get sector edge cases.")]
    public void GetSectorEdges()
    {
        TR1Level level = GetTR1TestLevel();
        TestSectorEdges(level.Rooms[0], 6656, 18944);
    }

    [TestMethod]
    [Description("Get a sector on a portal.")]
    public void GetSectorOnPortal()
    {
        TR1Level level = GetTR1TestLevel();
        TRRoom room = level.Rooms[0];
        TRRoomSector sector = room.GetSector(3, 0, TRUnit.Sector);

        Assert.AreEqual(42, room.Sectors.IndexOf(sector));
        Assert.AreNotEqual(0, sector.FDIndex);
        Assert.IsNotNull(level.FloorData[sector.FDIndex].Find(e => e is FDPortalEntry));
    }

    [TestMethod]
    public void GetFlatFloorHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetFloorHeight(6320, 19776, 0, level.Rooms, false);
        int sectorHeight = level.Rooms[0].GetSector(6320, 19776).Floor * TRConsts.Step1;
        Assert.AreEqual(sectorHeight, height);
    }

    [TestMethod]
    public void GetSlantedFloorHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetFloorHeight(9394, 19272, 0, level.Rooms, false);
        Assert.AreEqual(-90, height);
    }

    [TestMethod]
    public void GetFlatCeilingHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetCeilingHeight(6491, 20981, 0, level.Rooms, false);
        int sectorHeight = level.Rooms[0].GetSector(6491, 20981).Ceiling * TRConsts.Step1;
        Assert.AreEqual(sectorHeight, height);
    }

    [TestMethod]
    public void GetSlantedCeilingHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetCeilingHeight(5467, 20981, 0, level.Rooms, false);
        Assert.AreEqual(-2902, height);
    }

    [TestMethod]
    public void GetFlatWaterHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetHeight(15872, 13824, 8, level.Rooms, true);
        Assert.AreEqual(1024, height);
    }

    [TestMethod]
    public void GetSlopedWaterHeight()
    {
        TR1Level level = GetTR1TestLevel();
        int height = level.FloorData.GetHeight(15872, 12800, 8, level.Rooms, true);
        Assert.AreEqual(639, height);
    }

    [TestMethod]
    [Description("Add a trigger.")]
    public void AddTrigger()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[0].GetSector(3, 7, TRUnit.Sector);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        Assert.AreNotEqual(0, sector.FDIndex);

        ushort fdIndex = sector.FDIndex;

        List<FDEntry> entries = level.FloorData[fdIndex];
        Assert.AreEqual(0, entries.Count);

        FDTriggerEntry trigger = new()
        {
            TrigType = FDTrigType.Pad,
            OneShot = true,
            Mask = 1 << 2,
            Timer = 10,
            Actions = new()
            {
                new()
                {
                    Action = FDTrigAction.PlaySoundtrack,
                    Parameter = 8
                }
            }
        };
        entries.Add(trigger);

        level = WriteReadTempLevel(level);

        sector = level.Rooms[0].GetSector(3, 7, TRUnit.Sector);
        Assert.AreEqual(fdIndex, sector.FDIndex);

        entries = level.FloorData[fdIndex];
        Assert.AreEqual(1, entries.Count);

        Assert.IsTrue(entries[0] is FDTriggerEntry);

        FDTriggerEntry tmpTrigger = (FDTriggerEntry)entries[0];
        Assert.AreEqual(trigger.TrigType, tmpTrigger.TrigType);
        Assert.AreEqual(trigger.OneShot, tmpTrigger.OneShot);
        Assert.AreEqual(trigger.Mask, tmpTrigger.Mask);
        Assert.AreEqual(trigger.Timer, tmpTrigger.Timer);

        Assert.AreEqual(trigger.Actions.Count, tmpTrigger.Actions.Count);
        Assert.AreEqual(trigger.Actions[0].Action, tmpTrigger.Actions[0].Action);
        Assert.AreEqual(trigger.Actions[0].Parameter, tmpTrigger.Actions[0].Parameter);
    }

    [TestMethod]
    [Description("Remove a trigger.")]
    public void RemoveTrigger()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        Assert.AreNotEqual(0, sector.FDIndex);

        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        FDEntry trigger = entries.Find(e => e is FDTriggerEntry);
        Assert.IsNotNull(trigger);
        entries.Remove(trigger);

        level = WriteReadTempLevel(level);

        sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        entries = level.FloorData[sector.FDIndex];
        trigger = entries.Find(e => e is FDTriggerEntry);
        Assert.IsNull(trigger);
    }

    [TestMethod]
    [Description("Change a trigger's properties.")]
    public void ChangeTrigger()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        Assert.AreNotEqual(0, sector.FDIndex);

        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        FDEntry entry = entries.Find(e => e is FDTriggerEntry);
        Assert.IsNotNull(entry);

        FDTriggerEntry trigger = (FDTriggerEntry)entry;
        Assert.IsFalse(trigger.OneShot);
        Assert.AreNotEqual(10, trigger.Timer);
        Assert.AreNotEqual(1 << 2, trigger.Mask);
        Assert.AreNotEqual(FDTrigType.Pad, trigger.TrigType);
        Assert.AreEqual(1, trigger.Actions.Count);
        Assert.AreEqual(FDTrigAction.Object, trigger.Actions[0].Action);
        Assert.AreNotEqual(8, trigger.Actions[0].Parameter);

        trigger.OneShot = true;
        trigger.Timer = 10;
        trigger.Mask = 1 << 2;

        trigger.TrigType = FDTrigType.Pad;
        trigger.Actions[0].Action = FDTrigAction.PlaySoundtrack;
        trigger.Actions[0].Parameter = 8;
        trigger.Actions.Add(new()
        {
            Action = FDTrigAction.FlipMap,
            Parameter = 1
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        entries = level.FloorData[sector.FDIndex];
        trigger = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
        Assert.IsNotNull(trigger);

        Assert.IsTrue(trigger.OneShot);
        Assert.AreEqual(10, trigger.Timer);
        Assert.AreEqual(1 << 2, trigger.Mask);
        Assert.AreEqual(FDTrigType.Pad, trigger.TrigType);
        Assert.AreEqual(2, trigger.Actions.Count);
        Assert.AreEqual(FDTrigAction.PlaySoundtrack, trigger.Actions[0].Action);
        Assert.AreEqual(8, trigger.Actions[0].Parameter);
        Assert.AreEqual(FDTrigAction.FlipMap, trigger.Actions[1].Action);
        Assert.AreEqual(1, trigger.Actions[1].Parameter);
    }

    [TestMethod]
    [Description("Remove all floor data from a sector.")]
    public void RemoveFloorData()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        Assert.AreNotEqual(0, sector.FDIndex);

        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        Assert.AreNotEqual(0, entries.Count);
        entries.Clear();

        level = WriteReadTempLevel(level);

        sector = level.Rooms[0].GetSector(5, 11, TRUnit.Sector);
        Assert.AreEqual(0, sector.FDIndex);
    }

    [TestMethod]
    [Description("Test finding entity triggers.")]
    public void GetEntityTriggers()
    {
        TR1Level level = GetTR1TestLevel();
        TestGetEntityTriggers(level.FloorData, 1);
    }

    [TestMethod]
    [Description("Test finding secret triggers.")]
    public void GetSecretTriggers()
    {
        TR1Level level = GetTR1TestLevel();
        TestGetSecretTriggers(level.FloorData, 0);
    }

    [TestMethod]
    [Description("Test finding specific trigger actions.")]
    public void GetTriggerActions()
    {
        TR1Level level = GetTR1TestLevel();

        List<FDActionItem> musicActions = level.FloorData.GetActionItems(FDTrigAction.PlaySoundtrack);
        Assert.AreEqual(1, musicActions.Count);
    }

    [TestMethod]
    [Description("Test removing entity triggers.")]
    public void RemoveEntityTriggers()
    {
        TR1Level level = GetTR1TestLevel();

        List<FDTriggerEntry> triggers = level.FloorData.GetEntityTriggers(1);
        Assert.AreNotEqual(0, triggers.Count);

        level.FloorData.RemoveEntityTriggers(level.Rooms.SelectMany(r => r.Sectors), 1);

        triggers = level.FloorData.GetEntityTriggers(1);
        Assert.AreEqual(0, triggers.Count);

        level = WriteReadTempLevel(level);
        triggers = level.FloorData.GetEntityTriggers(1);
        Assert.AreEqual(0, triggers.Count);
    }

    [TestMethod]
    [Description("Test adding a kill Lara entry.")]
    public void AddKillLara()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDKillLaraEntry());

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDKillLaraEntry killer = entries.Find(e => e is FDKillLaraEntry) as FDKillLaraEntry;
        Assert.IsNotNull(killer);
    }

    [TestMethod]
    [Description("Test adding a portal.")]
    public void AddPortal()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDPortalEntry
        {
            Room = 1
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDPortalEntry portal = entries.Find(e => e is FDPortalEntry) as FDPortalEntry;
        Assert.IsNotNull(portal);
        Assert.AreEqual(1, portal.Room);
    }

    [TestMethod]
    [Description("Test adding a floor slant.")]
    public void AddFloorSlant()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDSlantEntry
        {
            Type = FDSlantType.Floor,
            XSlant = -1,
            ZSlant = 2
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDSlantEntry slant = entries.Find(e => e is FDSlantEntry) as FDSlantEntry;
        Assert.IsNotNull(slant);
        Assert.AreEqual(FDSlantType.Floor, slant.Type);
        Assert.AreEqual(-1, slant.XSlant);
        Assert.AreEqual(2, slant.ZSlant);
    }

    [TestMethod]
    [Description("Test adding a ceiling slant.")]
    public void AddCeilingSlant()
    {
        TR1Level level = GetTR1TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDSlantEntry
        {
            Type = FDSlantType.Ceiling,
            XSlant = 2,
            ZSlant = -3
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDSlantEntry slant = entries.Find(e => e is FDSlantEntry) as FDSlantEntry;
        Assert.IsNotNull(slant);
        Assert.AreEqual(FDSlantType.Ceiling, slant.Type);
        Assert.AreEqual(2, slant.XSlant);
        Assert.AreEqual(-3, slant.ZSlant);
    }
}
