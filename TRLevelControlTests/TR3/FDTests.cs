using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR3;

[TestClass]
[TestCategory("FD")]
public class FDTests : FDTestBase
{
    [TestMethod]
    [Description("Test adding a monkey swing.")]
    public void AddMonkeySwing()
    {
        TR3Level level = GetTR3TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDMonkeySwingEntry());

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDMonkeySwingEntry monkey = entries.Find(e => e is FDMonkeySwingEntry) as FDMonkeySwingEntry;
        Assert.IsNotNull(monkey);
    }

    [TestMethod]
    [Description("Test adding minecart entries.")]
    public void AddMinecart()
    {
        static void TestMinecart(FDMinecartType type)
        {
            TR3Level level = GetTR3TestLevel();

            TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
            Assert.AreEqual(0, sector.FDIndex);

            level.FloorData.CreateFloorData(sector);
            List<FDEntry> entries = level.FloorData[sector.FDIndex];
            entries.Add(new FDMinecartEntry
            {
                Type = type
            });

            level = WriteReadTempLevel(level);

            sector = level.Rooms[2].GetSector(4608, 6656);
            Assert.AreNotEqual(0, sector.FDIndex);

            FDMinecartEntry minecart = entries.Find(e => e is FDMinecartEntry) as FDMinecartEntry;
            Assert.IsNotNull(minecart);
            Assert.AreEqual(type, minecart.Type);
        }

        TestMinecart(FDMinecartType.Left);
        TestMinecart(FDMinecartType.Right);
    }

    [TestMethod]
    [Description("Test adding floor triangulation.")]
    public void AddFloorTriangulation()
    {
        TR3Level level = GetTR3TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDTriangulationEntry
        {
            Type = FDTriangulationType.FloorNWSE_Solid,
            C00 = 1,
            C01 = 2,
            C10 = 3,
            C11 = 4,
            H1 = 5,
            H2 = 6,
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDTriangulationEntry triangle = entries.Find(e => e is FDTriangulationEntry) as FDTriangulationEntry;
        Assert.IsNotNull(triangle);
        Assert.AreEqual(FDTriangulationType.FloorNWSE_Solid, triangle.Type);
        Assert.AreEqual(1, triangle.C00);
        Assert.AreEqual(2, triangle.C01);
        Assert.AreEqual(3, triangle.C10);
        Assert.AreEqual(4, triangle.C11);
        Assert.AreEqual(5, triangle.H1);
        Assert.AreEqual(6, triangle.H2);
    }

    [TestMethod]
    [Description("Test adding ceiling triangulation.")]
    public void AddCeilingTriangulation()
    {
        TR3Level level = GetTR3TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDTriangulationEntry
        {
            Type = FDTriangulationType.CeilingNWSE_NE,
            C00 = 6,
            C01 = 5,
            C10 = 4,
            C11 = 3,
            H1 = 2,
            H2 = 1,
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDTriangulationEntry triangle = entries.Find(e => e is FDTriangulationEntry) as FDTriangulationEntry;
        Assert.IsNotNull(triangle);
        Assert.AreEqual(FDTriangulationType.CeilingNWSE_NE, triangle.Type);
        Assert.AreEqual(6, triangle.C00);
        Assert.AreEqual(5, triangle.C01);
        Assert.AreEqual(4, triangle.C10);
        Assert.AreEqual(3, triangle.C11);
        Assert.AreEqual(2, triangle.H1);
        Assert.AreEqual(1, triangle.H2);
    }

    [TestMethod]
    [Description("Add invalid FD entries for TR3.")]
    public void AddInvalidEntries()
    {
        TR3Level level = GetTR3TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].AddRange(new List<FDEntry>
        {
            new FDBeetleEntry(),
            new FDDeferredTriggerEntry(),
        });

        WriteReadTempLevel(level);
        Assert.AreEqual(0, sector.FDIndex);
    }

    [TestMethod]
    [Description("Add and remove overlaps and verify only the related boxes are affected.")]
    public void ModifyOverlaps()
    {
        TR3Level level = GetTR3TestLevel();
        ModifyOverlaps(level, () => WriteReadTempLevel(level));
    }

    [TestMethod]
    [Description("Add a new box/zone and verify none of the original zones are affected.")]
    public void ModifyZones()
    {
        TR3Level level = GetTR3TestLevel();
        ModifyZones(level, () => WriteReadTempLevel(level));
    }
}
