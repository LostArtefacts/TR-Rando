using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR5;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    [TestMethod]
    [DataRow(TR5LevelNames.ROME)]
    [DataRow(TR5LevelNames.MARKETS)]
    [DataRow(TR5LevelNames.COLOSSEUM)]
    [DataRow(TR5LevelNames.BASE)]
    [DataRow(TR5LevelNames.SUBMARINE)]
    [DataRow(TR5LevelNames.DEEPSEA)]
    [DataRow(TR5LevelNames.SINKING)]
    [DataRow(TR5LevelNames.GALLOWS)]
    [DataRow(TR5LevelNames.LABYRINTH)]
    [DataRow(TR5LevelNames.MILL)]
    [DataRow(TR5LevelNames.FLOOR13)]
    [DataRow(TR5LevelNames.ESCAPE)]
    [DataRow(TR5LevelNames.REDALERT)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteTR5Level(levelName);
    }

    [TestMethod]
    [DataRow(TR5LevelNames.ROME)]
    [DataRow(TR5LevelNames.MARKETS)]
    [DataRow(TR5LevelNames.COLOSSEUM)]
    [DataRow(TR5LevelNames.BASE)]
    [DataRow(TR5LevelNames.SUBMARINE)]
    [DataRow(TR5LevelNames.DEEPSEA)]
    [DataRow(TR5LevelNames.SINKING)]
    [DataRow(TR5LevelNames.GALLOWS)]
    [DataRow(TR5LevelNames.LABYRINTH)]
    [DataRow(TR5LevelNames.MILL)]
    [DataRow(TR5LevelNames.FLOOR13)]
    [DataRow(TR5LevelNames.ESCAPE)]
    [DataRow(TR5LevelNames.REDALERT)]
    public void TestFloorData(string levelName)
    {
        TR5Level level = GetTR5Level(levelName);

        List<ushort> originalData = new(level.LevelDataChunk.FloorData);

        FDControl fdControl = new();
        fdControl.ParseFromLevel(level);
        fdControl.WriteToLevel(level);

        CollectionAssert.AreEqual(originalData, level.LevelDataChunk.FloorData);
    }
}
