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
    public void ReadWriteRome()
    {
        ReadWriteTR5Level(TR5LevelNames.ROME);
    }

    [TestMethod]
    public void ReadWriteMarkets()
    {
        ReadWriteTR5Level(TR5LevelNames.MARKETS);
    }

    [TestMethod]
    public void ReadWriteColosseum()
    {
        ReadWriteTR5Level(TR5LevelNames.COLOSSEUM);
    }

    [TestMethod]
    public void ReadWriteBase()
    {
        ReadWriteTR5Level(TR5LevelNames.BASE);
    }

    [TestMethod]
    public void ReadWriteSubmarine()
    {
        ReadWriteTR5Level(TR5LevelNames.SUBMARINE);
    }

    [TestMethod]
    public void ReadWriteDeepSea()
    {
        ReadWriteTR5Level(TR5LevelNames.DEEPSEA);
    }

    [TestMethod]
    public void ReadWriteSinking()
    {
        ReadWriteTR5Level(TR5LevelNames.SINKING);
    }

    [TestMethod]
    public void ReadWriteGallows()
    {
        ReadWriteTR5Level(TR5LevelNames.GALLOWS);
    }

    [TestMethod]
    public void ReadWriteLabyrinth()
    {
        ReadWriteTR5Level(TR5LevelNames.LABYRINTH);
    }

    [TestMethod]
    public void ReadWriteMill()
    {
        ReadWriteTR5Level(TR5LevelNames.MILL);
    }

    [TestMethod]
    public void ReadWriteFloor13()
    {
        ReadWriteTR5Level(TR5LevelNames.FLOOR13);
    }

    [TestMethod]
    public void ReadWriteEscape()
    {
        ReadWriteTR5Level(TR5LevelNames.ESCAPE);
    }

    [TestMethod]
    public void ReadWriteRedAlert()
    {
        ReadWriteTR5Level(TR5LevelNames.REDALERT);
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
