using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Base.Enums;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR1LevelNames.AsOrderedList);
    public static IEnumerable<object[]> GetBaseLevels() => GetLevelNames(TR1LevelNames.AsOrderedList.Except(TR1LevelNames.AsListGold));
    public static IEnumerable<object[]> GetGoldLevels() => GetLevelNames(TR1LevelNames.AsListGold);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR1);
    }

    //[TestMethod]
    //[DynamicData(nameof(GetBaseLevels), DynamicDataSourceType.Method)]
    //public void TestFloorData(string levelName)
    //{
    //    TR1Level level = GetTR1Level(levelName);

    //    // Store the original floordata from the level
    //    List<ushort> originalFData = new(level.FloorData);

    //    // Parse the floordata using FDControl and re-write the parsed data back
    //    FDControl fdControl = new();
    //    fdControl.ParseFromLevel(level);
    //    fdControl.WriteToLevel(level);

    //    // Compare to make sure the original fdata was written back.
    //    CollectionAssert.AreEqual(originalFData, level.FloorData, $"Floordata in {levelName} does not match after read/write.");
    //}

    //[TestMethod]
    //[DynamicData(nameof(GetGoldLevels), DynamicDataSourceType.Method)]
    //public void TestAgressiveFloorData(string levelName)
    //{
    //    // The UB levels seem to have been compiled with agressive FD packing.
    //    // Our library will expand and so byte-for-byte checks can't be done.
    //    // We will instead verify that every sector points to a valid FD entry.
    //    TR1Level level = GetTR1Level(levelName);

    //    FDControl fdControl = new();
    //    fdControl.ParseFromLevel(level);
    //    fdControl.WriteToLevel(level);

    //    foreach (TRRoomSector sector in level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0)))
    //    {
    //        Assert.IsTrue(fdControl.Entries.ContainsKey(sector.FDIndex));
    //    }
    //}

    [TestMethod]
    public void ModifyZonesTest()
    {
        TR1Level lvl = GetTR1Level(TR1LevelNames.CAVES);

        // For every box, store the current zone. We use the serialized form
        // for comparison.
        Dictionary<int, byte[]> flipOffZones = new();
        Dictionary<int, byte[]> flipOnZones = new();
        for (int i = 0; i < lvl.Boxes.Count; i++)
        {
            flipOffZones[i] = lvl.Zones[i][FlipStatus.Off].Serialize();
            flipOnZones[i] = lvl.Zones[i][FlipStatus.On].Serialize();
        }

        // Add a new box
        lvl.Boxes.Add(lvl.Boxes[0]);

        // Add a new zone for the box and store its serialized form for comparison
        int newBoxIndex = (int)(lvl.Boxes.Count - 1);
        TR1BoxUtilities.DuplicateZone(lvl, 0);
        flipOffZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.Off].Serialize();
        flipOnZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.On].Serialize();

        // Verify the number of zone ushorts matches what's expected for the box count
        Assert.AreEqual(TR1BoxUtilities.FlattenZones(lvl.Zones).Count, (int)(6 * lvl.Boxes.Count));

        // Write and re-read the level
        lvl = WriteReadTempLevel(lvl);

        // Capture all of the zones again. Make sure the addition of the zone above didn't
        // affect any of the others and that the addition itself matches after IO.
        for (int i = 0; i < lvl.Boxes.Count; i++)
        {
            byte[] flipOff = lvl.Zones[i][FlipStatus.Off].Serialize();
            Assert.IsTrue(flipOffZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOffZones[i], flipOff);

            byte[] flipOn = lvl.Zones[i][FlipStatus.On].Serialize();
            Assert.IsTrue(flipOnZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOnZones[i], flipOn);
        }
    }
}
