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
    public void Floordata_ReadWrite_DefaultTest()
    {
        TR5Level lvl = GetTR5Level(TR5LevelNames.ROME);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.LevelDataChunk.NumFloorData];
        Array.Copy(lvl.LevelDataChunk.Floordata, originalFData, lvl.LevelDataChunk.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.LevelDataChunk.Floordata;

        for (int i = 0; i < originalFData.Length; i++)
        {
            if (originalFData[i] != newFData[i])
            {
                break;
            }
        }

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.LevelDataChunk.NumFloorData);

        foreach (TR5Room room in lvl.LevelDataChunk.Rooms)
        {
            Assert.IsTrue(room.RoomData.FlattenLightsBulbsAndSectors());
        }
    }
}
