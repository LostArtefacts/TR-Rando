using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
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
    public void TestReadWrite(string levelname)
    {
        ReadWriteLevel(levelname, TRGameVersion.TR4);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestFloorData(string levelName)
    {
        TR4Level level = GetTR4Level(levelName);

        List<ushort> originalData = new(level.FloorData);

        FDControl fdControl = new();
        fdControl.ParseFromLevel(level);
        fdControl.WriteToLevel(level);

        CollectionAssert.AreEqual(originalData, level.FloorData);
    }

    [TestMethod]
    public void Floordata_ReadWrite_MechBeetleTest()
    {
        TR4Level lvl = GetTR4Level(TR4LevelNames.CLEOPATRA);

        //Store the original floordata from the level
        List<ushort> originalFData = new(lvl.FloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");
    }

    [TestMethod]
    public void Floordata_ReadWrite_TriggerTriggererTest()
    {
        TR4Level lvl = GetTR4Level(TR4LevelNames.ALEXANDRIA);

        //Store the original floordata from the level
        List<ushort> originalFData = new(lvl.FloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");
    }
}
