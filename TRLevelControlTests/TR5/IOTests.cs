using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR5;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR5LevelNames.AsList);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR5);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestFloorData(string levelName)
    {
        TR5Level level = GetTR5Level(levelName);

        List<ushort> originalData = new(level.FloorData);

        FDControl fdControl = new();
        fdControl.ParseFromLevel(level);
        fdControl.WriteToLevel(level);

        CollectionAssert.AreEqual(originalData, level.FloorData);
    }
}
