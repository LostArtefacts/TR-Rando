using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using TRImageControl;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR2;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR2LevelNames.AsOrderedList);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR2);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestAgressiveFloorData(string levelName)
    {
        TR2Level level = GetTR2Level(levelName);
        IEnumerable<TRRoomSector> allFDSectors = level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0));

        foreach (TRRoomSector sector in allFDSectors)
        {
            Assert.IsTrue(level.FloorData.ContainsKey(sector.FDIndex));
        }
        Assert.AreEqual(allFDSectors.Count(), allFDSectors.DistinctBy(s => s.FDIndex).Count());
    }

    [TestMethod]
    public void ModifyTexturesTest()
    {
        TR2Level lvl = GetTR2Level(TR2LevelNames.MONASTERY);

        TR2LevelControl control = new();
        using MemoryStream ms1 = new();
        using MemoryStream ms2 = new();

        // Store the untouched raw data
        control.Write(lvl, ms1);
        byte[] lvlAsBytes = ms1.ToArray();

        // Convert each tile to a bitmap, and then convert it back
        foreach (TRTexImage16 tile in lvl.Images16)
        {
            using Bitmap bmp = tile.ToBitmap();
            tile.Pixels = TextureUtilities.ImportFromBitmap(bmp);
        }

        control.Write(lvl, ms2);
        byte[] lvlAfterWrite = ms2.ToArray();

        // Confirm the raw data still matches
        CollectionAssert.AreEqual(lvlAsBytes, lvlAfterWrite, "Read does not match byte for byte");
    }
}
