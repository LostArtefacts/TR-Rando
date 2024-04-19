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
    [DataRow(TR4LevelNames.ANGKOR)]
    [DataRow(TR4LevelNames.IRIS_RACE)]
    [DataRow(TR4LevelNames.SETH)]
    [DataRow(TR4LevelNames.BURIAL)]
    [DataRow(TR4LevelNames.VALLEY)]
    [DataRow(TR4LevelNames.KV5)]
    [DataRow(TR4LevelNames.KARNAK)]
    [DataRow(TR4LevelNames.HYPOSTYLE)]
    [DataRow(TR4LevelNames.LAKE)]
    [DataRow(TR4LevelNames.SEMERKHET)]
    [DataRow(TR4LevelNames.GUARDIAN)]
    [DataRow(TR4LevelNames.TRAIN)]
    [DataRow(TR4LevelNames.ALEXANDRIA)]
    [DataRow(TR4LevelNames.COASTAL)]
    [DataRow(TR4LevelNames.PHAROS)]
    [DataRow(TR4LevelNames.CLEOPATRA)]
    [DataRow(TR4LevelNames.CATACOMBS)]
    [DataRow(TR4LevelNames.POSEIDON)]
    [DataRow(TR4LevelNames.LIBRARY)]
    [DataRow(TR4LevelNames.DEMETRIUS)]
    [DataRow(TR4LevelNames.CITY)]
    [DataRow(TR4LevelNames.TRENCHES)]
    [DataRow(TR4LevelNames.TULUN)]
    [DataRow(TR4LevelNames.BAZAAR)]
    [DataRow(TR4LevelNames.GATE)]
    [DataRow(TR4LevelNames.CITADEL)]
    [DataRow(TR4LevelNames.SPHINX_COMPLEX)]
    [DataRow(TR4LevelNames.SPHINX_UNUSED)]
    [DataRow(TR4LevelNames.SPHINX_UNDER)]
    [DataRow(TR4LevelNames.MENKAURE)]
    [DataRow(TR4LevelNames.MENKAURE_INSIDE)]
    [DataRow(TR4LevelNames.MASTABAS)]
    [DataRow(TR4LevelNames.PYRAMID_OUT)]
    [DataRow(TR4LevelNames.KHUFU)]
    [DataRow(TR4LevelNames.PYRAMID_IN)]
    [DataRow(TR4LevelNames.HORUS1)]
    [DataRow(TR4LevelNames.HORUS2)]
    [DataRow(TR4LevelNames.TIMES)]
    public void TestReadWrite(string levelname)
    {
        ReadWriteLevel(levelname, TRGameVersion.TR4);
    }

    [TestMethod]
    [DataRow(TR4LevelNames.ANGKOR)]
    [DataRow(TR4LevelNames.IRIS_RACE)]
    [DataRow(TR4LevelNames.SETH)]
    [DataRow(TR4LevelNames.BURIAL)]
    [DataRow(TR4LevelNames.VALLEY)]
    [DataRow(TR4LevelNames.KV5)]
    [DataRow(TR4LevelNames.KARNAK)]
    [DataRow(TR4LevelNames.HYPOSTYLE)]
    [DataRow(TR4LevelNames.LAKE)]
    [DataRow(TR4LevelNames.SEMERKHET)]
    [DataRow(TR4LevelNames.GUARDIAN)]
    [DataRow(TR4LevelNames.TRAIN)]
    [DataRow(TR4LevelNames.ALEXANDRIA)]
    [DataRow(TR4LevelNames.COASTAL)]
    [DataRow(TR4LevelNames.PHAROS)]
    [DataRow(TR4LevelNames.CLEOPATRA)]
    [DataRow(TR4LevelNames.CATACOMBS)]
    [DataRow(TR4LevelNames.POSEIDON)]
    [DataRow(TR4LevelNames.LIBRARY)]
    [DataRow(TR4LevelNames.DEMETRIUS)]
    [DataRow(TR4LevelNames.CITY)]
    [DataRow(TR4LevelNames.TRENCHES)]
    [DataRow(TR4LevelNames.TULUN)]
    [DataRow(TR4LevelNames.BAZAAR)]
    [DataRow(TR4LevelNames.GATE)]
    [DataRow(TR4LevelNames.CITADEL)]
    [DataRow(TR4LevelNames.SPHINX_COMPLEX)]
    [DataRow(TR4LevelNames.SPHINX_UNUSED)]
    [DataRow(TR4LevelNames.SPHINX_UNDER)]
    [DataRow(TR4LevelNames.MENKAURE)]
    [DataRow(TR4LevelNames.MENKAURE_INSIDE)]
    [DataRow(TR4LevelNames.MASTABAS)]
    [DataRow(TR4LevelNames.PYRAMID_OUT)]
    [DataRow(TR4LevelNames.KHUFU)]
    [DataRow(TR4LevelNames.PYRAMID_IN)]
    [DataRow(TR4LevelNames.HORUS1)]
    [DataRow(TR4LevelNames.HORUS2)]
    [DataRow(TR4LevelNames.TIMES)]
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
