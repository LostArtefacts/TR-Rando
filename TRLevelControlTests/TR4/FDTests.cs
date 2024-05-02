using Microsoft.VisualStudio.TestTools.UnitTesting;
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
}
