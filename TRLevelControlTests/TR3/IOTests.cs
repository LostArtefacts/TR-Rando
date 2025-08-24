using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR3;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR3LevelNames.AsOrderedList);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestOGReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3, false);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestRemasteredReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3, true);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestPDPReadWrite(string levelName)
    {
        ReadWritePDP(levelName, TRGameVersion.TR3);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestMAPReadWrite(string levelName)
    {
        ReadWriteMAP(levelName, TRGameVersion.TR3);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestTRGReadWrite(string levelName)
    {
        ReadWriteTRG(levelName, TRGameVersion.TR3);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestAgressiveFloorData(string levelName)
    {
        TR3Level level = GetTR3Level(levelName);
        IEnumerable<TRRoomSector> allFDSectors = level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0));

        foreach (TRRoomSector sector in allFDSectors)
        {
            Assert.IsTrue(level.FloorData.ContainsKey(sector.FDIndex));
        }
        Assert.AreEqual(allFDSectors.Count(), allFDSectors.DistinctBy(s => s.FDIndex).Count());
    }
}
