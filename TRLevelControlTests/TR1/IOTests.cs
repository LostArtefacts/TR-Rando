using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
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
    [DataRow(TRLevelNames.ASSAULT)]
    [DataRow(TRLevelNames.CAVES)]
    [DataRow(TRLevelNames.VILCABAMBA)]
    [DataRow(TRLevelNames.VALLEY)]
    [DataRow(TRLevelNames.QUALOPEC)]
    [DataRow(TRLevelNames.QUALOPEC_CUT)]
    [DataRow(TRLevelNames.FOLLY)]
    [DataRow(TRLevelNames.COLOSSEUM)]
    [DataRow(TRLevelNames.MIDAS)]
    [DataRow(TRLevelNames.CISTERN)]
    [DataRow(TRLevelNames.TIHOCAN)]
    [DataRow(TRLevelNames.TIHOCAN_CUT)]
    [DataRow(TRLevelNames.KHAMOON)]
    [DataRow(TRLevelNames.OBELISK)]
    [DataRow(TRLevelNames.SANCTUARY)]
    [DataRow(TRLevelNames.MINES)]
    [DataRow(TRLevelNames.MINES_CUT)]
    [DataRow(TRLevelNames.ATLANTIS)]
    [DataRow(TRLevelNames.ATLANTIS_CUT)]
    [DataRow(TRLevelNames.PYRAMID)]
    [DataRow(TRLevelNames.EGYPT)]
    [DataRow(TRLevelNames.CAT)]
    [DataRow(TRLevelNames.HIVE)]
    [DataRow(TRLevelNames.STRONGHOLD)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR1);
    }

    [TestMethod]
    public void Floordata_ReadWrite_DefaultTest()
    {
        TRLevel lvl = GetTR1Level(TRLevelNames.ATLANTIS);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.NumFloorData];
        Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
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
        TRLevel lvl = GetTR1Level(TRLevelNames.CAVES);

        // For every box, store the current zone. We use the serialized form
        // for comparison.
        Dictionary<int, byte[]> flipOffZones = new Dictionary<int, byte[]>();
        Dictionary<int, byte[]> flipOnZones = new Dictionary<int, byte[]>();
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
        lvl = WriteReadTempLevel(lvl, TRLevelNames.CAVES);

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
        TRLevel lvl = GetTR1Level(TRLevelNames.CAVES);

        byte[] lvlBeforeSort = lvl.Serialize();

        SoundUtilities.ResortSoundIndices(lvl);

        byte[] lvlAfterSort = lvl.Serialize();

        CollectionAssert.AreEqual(lvlBeforeSort, lvlAfterSort);
    }
}
