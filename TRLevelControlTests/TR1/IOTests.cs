using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR1LevelNames.AsOrderedList);
    public static IEnumerable<object[]> GetBaseLevels() => GetLevelNames(TR1LevelNames.AsOrderedList.Except(TR1LevelNames.AsListGold));
    public static IEnumerable<object[]> GetGoldLevels() => GetLevelNames(TR1LevelNames.AsListGold);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR1);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestPDPReadWrite(string levelName)
    {
        ReadWritePDP(levelName, TRGameVersion.TR1);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestAgressiveFloorData(string levelName)
    {
        // The UB levels seem to have been compiled with agressive FD packing. Our library will expand and so byte-for-byte checks
        // can't be done when not using the observer approach. We will instead verify that every sector points to a valid FD entry
        // and that the expansion works by eliminating duplicates.
        TR1Level level = GetTR1Level(levelName);
        IEnumerable<TRRoomSector> allFDSectors = level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0));

        foreach (TRRoomSector sector in allFDSectors)
        {
            Assert.IsTrue(level.FloorData.ContainsKey(sector.FDIndex));
        }
        Assert.AreEqual(allFDSectors.Count(), allFDSectors.DistinctBy(s => s.FDIndex).Count());
    }
}
