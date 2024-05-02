using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    //[TestMethod]
    //public void Floordata_ReadWrite_MechBeetleTest()
    //{
    //    TR4Level lvl = GetTR4Level(TR4LevelNames.CLEOPATRA);

    //    //Store the original floordata from the level
    //    List<ushort> originalFData = new(lvl.FloorData);

    //    //Parse the floordata using FDControl and re-write the parsed data back
    //    FDControl fdataReader = new();
    //    fdataReader.ParseFromLevel(lvl);
    //    fdataReader.WriteToLevel(lvl);

    //    //Compare to make sure the original fdata was written back.
    //    CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");
    //}

    //[TestMethod]
    //public void Floordata_ReadWrite_TriggerTriggererTest()
    //{
    //    TR4Level lvl = GetTR4Level(TR4LevelNames.ALEXANDRIA);

    //    //Store the original floordata from the level
    //    List<ushort> originalFData = new(lvl.FloorData);

    //    //Parse the floordata using FDControl and re-write the parsed data back
    //    FDControl fdataReader = new();
    //    fdataReader.ParseFromLevel(lvl);
    //    fdataReader.WriteToLevel(lvl);

    //    //Compare to make sure the original fdata was written back.
    //    CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");
    //}
}
