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
    public void ReadWriteGym()
    {
        ReadWriteTR1Level(TRLevelNames.ASSAULT);
    }

    [TestMethod]
    public void ReadWriteCaves()
    {
        ReadWriteTR1Level(TRLevelNames.CAVES);
    }

    [TestMethod]
    public void ReadWriteVilcabamba()
    {
        ReadWriteTR1Level(TRLevelNames.VILCABAMBA);
    }

    [TestMethod]
    public void ReadWriteValley()
    {
        ReadWriteTR1Level(TRLevelNames.VALLEY);
    }

    [TestMethod]
    public void ReadWriteToQ()
    {
        ReadWriteTR1Level(TRLevelNames.QUALOPEC);
    }

    [TestMethod]
    public void ReadWriteCut1()
    {
        ReadWriteTR1Level(TRLevelNames.QUALOPEC_CUT);
    }

    [TestMethod]
    public void ReadWriteFolly()
    {
        ReadWriteTR1Level(TRLevelNames.FOLLY);
    }

    [TestMethod]
    public void ReadWriteColosseum()
    {
        ReadWriteTR1Level(TRLevelNames.COLOSSEUM);
    }

    [TestMethod]
    public void ReadWriteMidas()
    {
        ReadWriteTR1Level(TRLevelNames.MIDAS);
    }

    [TestMethod]
    public void ReadWriteCistern()
    {
        ReadWriteTR1Level(TRLevelNames.CISTERN);
    }

    [TestMethod]
    public void ReadWriteToT()
    {
        ReadWriteTR1Level(TRLevelNames.TIHOCAN);
    }

    [TestMethod]
    public void ReadWriteCut2()
    {
        ReadWriteTR1Level(TRLevelNames.TIHOCAN_CUT);
    }

    [TestMethod]
    public void ReadWriteKhamoon()
    {
        ReadWriteTR1Level(TRLevelNames.KHAMOON);
    }

    [TestMethod]
    public void ReadWriteObelisk()
    {
        ReadWriteTR1Level(TRLevelNames.OBELISK);
    }

    [TestMethod]
    public void ReadWriteSanctuary()
    {
        ReadWriteTR1Level(TRLevelNames.SANCTUARY);
    }

    [TestMethod]
    public void ReadWriteMines()
    {
        ReadWriteTR1Level(TRLevelNames.MINES);
    }

    [TestMethod]
    public void ReadWriteCut3()
    {
        ReadWriteTR1Level(TRLevelNames.MINES_CUT);
    }

    [TestMethod]
    public void ReadWriteAtlantis()
    {
        ReadWriteTR1Level(TRLevelNames.ATLANTIS);
    }

    [TestMethod]
    public void ReadWriteCut4()
    {
        ReadWriteTR1Level(TRLevelNames.ATLANTIS_CUT);
    }

    [TestMethod]
    public void ReadWritePyramid()
    {
        ReadWriteTR1Level(TRLevelNames.PYRAMID);
    }

    [TestMethod]
    public void ReadWriteEgypt()
    {
        ReadWriteTR1Level(TRLevelNames.EGYPT);
    }

    [TestMethod]
    public void ReadWriteCat()
    {
        ReadWriteTR1Level(TRLevelNames.CAT);
    }

    [TestMethod]
    public void ReadWriteHive()
    {
        ReadWriteTR1Level(TRLevelNames.HIVE);
    }

    [TestMethod]
    public void ReadWriteStronghold()
    {
        ReadWriteTR1Level(TRLevelNames.STRONGHOLD);
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
