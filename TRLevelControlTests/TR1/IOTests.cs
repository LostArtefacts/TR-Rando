using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Base.Enums;
using TRModelTransporter.Handlers;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    [TestMethod]
    [DataRow(TR1LevelNames.ASSAULT)]
    [DataRow(TR1LevelNames.CAVES)]
    [DataRow(TR1LevelNames.VILCABAMBA)]
    [DataRow(TR1LevelNames.VALLEY)]
    [DataRow(TR1LevelNames.QUALOPEC)]
    [DataRow(TR1LevelNames.QUALOPEC_CUT)]
    [DataRow(TR1LevelNames.FOLLY)]
    [DataRow(TR1LevelNames.COLOSSEUM)]
    [DataRow(TR1LevelNames.MIDAS)]
    [DataRow(TR1LevelNames.CISTERN)]
    [DataRow(TR1LevelNames.TIHOCAN)]
    [DataRow(TR1LevelNames.TIHOCAN_CUT)]
    [DataRow(TR1LevelNames.KHAMOON)]
    [DataRow(TR1LevelNames.OBELISK)]
    [DataRow(TR1LevelNames.SANCTUARY)]
    [DataRow(TR1LevelNames.MINES)]
    [DataRow(TR1LevelNames.MINES_CUT)]
    [DataRow(TR1LevelNames.ATLANTIS)]
    [DataRow(TR1LevelNames.ATLANTIS_CUT)]
    [DataRow(TR1LevelNames.PYRAMID)]
    [DataRow(TR1LevelNames.EGYPT)]
    [DataRow(TR1LevelNames.CAT)]
    [DataRow(TR1LevelNames.HIVE)]
    [DataRow(TR1LevelNames.STRONGHOLD)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR1);
    }

    [TestMethod]
    public void Floordata_ReadWrite_DefaultTest()
    {
        TR1Level lvl = GetTR1Level(TR1LevelNames.ATLANTIS);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.NumFloorData];
        Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.FloorData;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.NumFloorData);
    }

    [TestMethod]
    public void ModifyZonesTest()
    {
        TR1Level lvl = GetTR1Level(TR1LevelNames.CAVES);

        // For every box, store the current zone. We use the serialized form
        // for comparison.
        Dictionary<int, byte[]> flipOffZones = new();
        Dictionary<int, byte[]> flipOnZones = new();
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            flipOffZones[i] = lvl.Zones[i][FlipStatus.Off].Serialize();
            flipOnZones[i] = lvl.Zones[i][FlipStatus.On].Serialize();
        }

        // Add a new box
        List<TRBox> boxes = lvl.Boxes.ToList();
        boxes.Add(boxes[0]);
        lvl.Boxes = boxes.ToArray();
        lvl.NumBoxes++;

        // Add a new zone for the box and store its serialized form for comparison
        int newBoxIndex = (int)(lvl.NumBoxes - 1);
        TR1BoxUtilities.DuplicateZone(lvl, 0);
        flipOffZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.Off].Serialize();
        flipOnZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.On].Serialize();

        // Verify the number of zone ushorts matches what's expected for the box count
        Assert.AreEqual(TR1BoxUtilities.FlattenZones(lvl.Zones).Length, (int)(6 * lvl.NumBoxes));

        // Write and re-read the level
        lvl = WriteReadTempLevel(lvl);

        // Capture all of the zones again. Make sure the addition of the zone above didn't
        // affect any of the others and that the addition itself matches after IO.
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            byte[] flipOff = lvl.Zones[i][FlipStatus.Off].Serialize();
            Assert.IsTrue(flipOffZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOffZones[i], flipOff);

            byte[] flipOn = lvl.Zones[i][FlipStatus.On].Serialize();
            Assert.IsTrue(flipOnZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOnZones[i], flipOn);
        }
    }

    [TestMethod]
    public void ResortSoundsTest()
    {
        TR1Level lvl = GetTR1Level(TR1LevelNames.CAVES);

        TR1LevelControl control = new();
        using MemoryStream ms1 = new();
        using MemoryStream ms2 = new();

        control.Write(lvl, ms1);
        byte[] lvlBeforeSort = ms1.ToArray();

        SoundUtilities.ResortSoundIndices(lvl);

        control.Write(lvl, ms2);
        byte[] lvlAfterSort = ms2.ToArray();

        CollectionAssert.AreEqual(lvlBeforeSort, lvlAfterSort);
    }
}
