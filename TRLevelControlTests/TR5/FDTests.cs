using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR5;

[TestClass]
[TestCategory("FD")]
public class FDTests : FDTestBase
{
    [TestMethod]
    [Description("Add invalid FD entries for TR5.")]
    public void AddInvalidEntries()
    {
        TR5Level level = GetTR5TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].AddRange(new List<FDEntry>
        {
            new FDMinecartEntry(),
        });

        WriteReadTempLevel(level);
        Assert.AreEqual(0, sector.FDIndex);
    }

    [TestMethod]
    [Description("Add and remove overlaps and verify only the related boxes are affected.")]
    public void ModifyOverlaps()
    {
        TR5Level level = GetTR5TestLevel();
        ModifyOverlaps(level, () => WriteReadTempLevel(level));
    }
}
