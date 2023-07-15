using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR4;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    [TestMethod]
    public void ReadWriteAngkor()
    {
        ReadWriteTR4Level(TR4LevelNames.ANGKOR);
    }

    [TestMethod]
    public void ReadWriteIris()
    {
        ReadWriteTR4Level(TR4LevelNames.IRIS_RACE);
    }

    [TestMethod]
    public void ReadWriteSeth()
    {
        ReadWriteTR4Level(TR4LevelNames.SETH);
    }

    [TestMethod]
    public void ReadWriteBurial()
    {
        ReadWriteTR4Level(TR4LevelNames.BURIAL);
    }

    [TestMethod]
    public void ReadWriteValley()
    {
        ReadWriteTR4Level(TR4LevelNames.VALLEY);
    }

    [TestMethod]
    public void ReadWriteKV5()
    {
        ReadWriteTR4Level(TR4LevelNames.KV5);
    }

    [TestMethod]
    public void ReadWriteKarnak()
    {
        ReadWriteTR4Level(TR4LevelNames.KARNAK);
    }

    [TestMethod]
    public void ReadWriteHypostyle()
    {
        ReadWriteTR4Level(TR4LevelNames.HYPOSTYLE);
    }

    [TestMethod]
    public void ReadWriteLake()
    {
        ReadWriteTR4Level(TR4LevelNames.LAKE);
    }

    [TestMethod]
    public void ReadWriteSemerkhet()
    {
        ReadWriteTR4Level(TR4LevelNames.SEMERKHET);
    }

    [TestMethod]
    public void ReadWriteGuardian()
    {
        ReadWriteTR4Level(TR4LevelNames.GUARDIAN);
    }

    [TestMethod]
    public void ReadWriteTrain()
    {
        ReadWriteTR4Level(TR4LevelNames.TRAIN);
    }

    [TestMethod]
    public void ReadWriteAlexandria()
    {
        ReadWriteTR4Level(TR4LevelNames.ALEXANDRIA);
    }

    [TestMethod]
    public void ReadWriteCoastal()
    {
        ReadWriteTR4Level(TR4LevelNames.COASTAL);
    }

    [TestMethod]
    public void ReadWriteCatacombs()
    {
        ReadWriteTR4Level(TR4LevelNames.CATACOMBS);
    }

    [TestMethod]
    public void ReadWritePoseidon()
    {
        ReadWriteTR4Level(TR4LevelNames.POSEIDON);
    }

    [TestMethod]
    public void ReadWriteLibrary()
    {
        ReadWriteTR4Level(TR4LevelNames.LIBRARY);
    }

    [TestMethod]
    public void ReadWriteDemetrius()
    {
        ReadWriteTR4Level(TR4LevelNames.DEMETRIUS);
    }

    [TestMethod]
    public void ReadWritePharos()
    {
        ReadWriteTR4Level(TR4LevelNames.PHAROS);
    }

    [TestMethod]
    public void ReadWriteCleopatra()
    {
        ReadWriteTR4Level(TR4LevelNames.CLEOPATRA);
    }

    [TestMethod]
    public void ReadWriteCity()
    {
        ReadWriteTR4Level(TR4LevelNames.CITY);
    }

    [TestMethod]
    public void ReadWriteTulun()
    {
        ReadWriteTR4Level(TR4LevelNames.TULUN);
    }

    [TestMethod]
    public void ReadWriteCitadelGate()
    {
        ReadWriteTR4Level(TR4LevelNames.GATE);
    }

    [TestMethod]
    public void ReadWriteTrenches()
    {
        ReadWriteTR4Level(TR4LevelNames.TRENCHES);
    }

    [TestMethod]
    public void ReadWriteBazaar()
    {
        ReadWriteTR4Level(TR4LevelNames.BAZAAR);
    }

    [TestMethod]
    public void ReadWriteCitadel()
    {
        ReadWriteTR4Level(TR4LevelNames.CITADEL);
    }

    [TestMethod]
    public void ReadWriteSphinxComplex()
    {
        ReadWriteTR4Level(TR4LevelNames.SPHINX_COMPLEX);
    }

    [TestMethod]
    public void ReadWriteSphinxExtra()
    {
        ReadWriteTR4Level(TR4LevelNames.SPHINX_UNUSED);
    }

    [TestMethod]
    public void ReadWriteSphinxUnder()
    {
        ReadWriteTR4Level(TR4LevelNames.SPHINX_UNDER);
    }

    [TestMethod]
    public void ReadWriteMenkaure()
    {
        ReadWriteTR4Level(TR4LevelNames.MENKAURE);
    }

    [TestMethod]
    public void ReadWriteMenkaureInside()
    {
        ReadWriteTR4Level(TR4LevelNames.MENKAURE_INSIDE);
    }

    [TestMethod]
    public void ReadWriteMastabas()
    {
        ReadWriteTR4Level(TR4LevelNames.MASTABAS);
    }

    [TestMethod]
    public void ReadWritePyramidOut()
    {
        ReadWriteTR4Level(TR4LevelNames.PYRAMID_OUT);
    }

    [TestMethod]
    public void ReadWriteKhufu()
    {
        ReadWriteTR4Level(TR4LevelNames.KHUFU);
    }

    [TestMethod]
    public void ReadWritePyramidIn()
    {
        ReadWriteTR4Level(TR4LevelNames.PYRAMID_IN);
    }

    [TestMethod]
    public void ReadWriteHorus1()
    {
        ReadWriteTR4Level(TR4LevelNames.HORUS1);
    }

    [TestMethod]
    public void ReadWriteHorus2()
    {
        ReadWriteTR4Level(TR4LevelNames.HORUS2);
    }

    [TestMethod]
    public void ReadWriteTimes()
    {
        ReadWriteTR4Level(TR4LevelNames.TIMES);
    }

    [TestMethod]
    public void Floordata_ReadWrite_DefaultTest()
    {
        TR4Level lvl = GetTR4Level(TR4LevelNames.TRAIN);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.LevelDataChunk.NumFloorData];
        Array.Copy(lvl.LevelDataChunk.Floordata, originalFData, lvl.LevelDataChunk.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.LevelDataChunk.Floordata;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.LevelDataChunk.NumFloorData);
    }

    [TestMethod]
    public void Floordata_ReadWrite_MechBeetleTest()
    {
        TR4Level lvl = GetTR4Level(TR4LevelNames.CLEOPATRA);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.LevelDataChunk.NumFloorData];
        Array.Copy(lvl.LevelDataChunk.Floordata, originalFData, lvl.LevelDataChunk.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.LevelDataChunk.Floordata;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.LevelDataChunk.NumFloorData);
    }

    [TestMethod]
    public void Floordata_ReadWrite_TriggerTriggererTest()
    {
        TR4Level lvl = GetTR4Level(TR4LevelNames.ALEXANDRIA);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.LevelDataChunk.NumFloorData];
        Array.Copy(lvl.LevelDataChunk.Floordata, originalFData, lvl.LevelDataChunk.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.LevelDataChunk.Floordata;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.LevelDataChunk.NumFloorData);
    }
}
