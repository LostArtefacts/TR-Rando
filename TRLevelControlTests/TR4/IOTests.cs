using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR4;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR4LevelNames.AsOrderedList);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestOGReadWrite(string levelname)
    {
        ReadWriteLevel(levelname, TRGameVersion.TR4, false);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestRemasteredReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR4, true);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestPDPReadWrite(string levelName)
    {
        ReadWritePDP(levelName, TRGameVersion.TR4);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestAgressiveFloorData(string levelName)
    {
        TR4Level level = GetTR4Level(levelName);
        IEnumerable<TRRoomSector> allFDSectors = level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0));

        foreach (TRRoomSector sector in allFDSectors)
        {
            Assert.IsTrue(level.FloorData.ContainsKey(sector.FDIndex));
        }
        Assert.AreEqual(allFDSectors.Count(), allFDSectors.DistinctBy(s => s.FDIndex).Count());
    }
}
