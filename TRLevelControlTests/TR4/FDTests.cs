using TRLevelControl.Model;

namespace TRLevelControlTests.TR4;

[TestClass]
[TestCategory("FD")]
public class FDTests : FDTestBase
{
    [TestMethod]
    [Description("Test adding a beetle entry.")]
    public void AddBeetle()
    {
        TR4Level level = GetTR4TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDBeetleEntry());

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDBeetleEntry beetle = entries.Find(e => e is FDBeetleEntry) as FDBeetleEntry;
        Assert.IsNotNull(beetle);
    }

    [TestMethod]
    [Description("Test adding a deferred trigger entry.")]
    public void AddDeferredTrigger()
    {
        TR4Level level = GetTR4TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDDeferredTriggerEntry());

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDDeferredTriggerEntry trig = entries.Find(e => e is FDDeferredTriggerEntry) as FDDeferredTriggerEntry;
        Assert.IsNotNull(trig);
    }

    [TestMethod]
    [Description("Add invalid FD entries for TR4.")]
    public void AddInvalidEntries()
    {
        TR4Level level = GetTR4TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].AddRange(new List<FDEntry>
        {
            new FDMinecartEntry(),
        });

        WriteReadTempLevel(level);
        Assert.AreEqual(0, sector.FDIndex);
    }

    [TestMethod]
    [Description("Add and remove overlaps and verify only the related boxes are affected.")]
    public void ModifyOverlaps()
    {
        TR4Level level = GetTR4TestLevel();
        ModifyOverlaps(level, () => WriteReadTempLevel(level));
    }

    [TestMethod]
    [Description("Add a new box/zone and verify none of the original zones are affected.")]
    public void ModifyZones()
    {
        TR4Level level = GetTR4TestLevel();
        ModifyZones(level, () => WriteReadTempLevel(level));
    }
}
