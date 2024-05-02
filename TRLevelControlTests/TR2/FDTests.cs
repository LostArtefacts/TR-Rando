using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR2;

[TestClass]
[TestCategory("FD")]
public class FDTests : FDTestBase
{
    [TestMethod]
    [Description("Test adding a ladder.")]
    public void AddLadder()
    {
        TR2Level level = GetTR2TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDClimbEntry
        {
            Direction = FDClimbDirection.PositiveX
        });

        level = WriteReadTempLevel(level);

        sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreNotEqual(0, sector.FDIndex);

        FDClimbEntry ladder = entries.Find(e => e is FDClimbEntry) as FDClimbEntry;
        Assert.IsNotNull(ladder);

        Assert.AreEqual(FDClimbDirection.PositiveX, ladder.Direction);
    }

    [TestMethod]
    [Description("Test changing a ladder.")]
    public void ChangeLadder()
    {
        TR2Level level = GetTR2TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        level.FloorData.CreateFloorData(sector);
        List<FDEntry> entries = level.FloorData[sector.FDIndex];
        entries.Add(new FDClimbEntry());

        void TestDirection(FDClimbDirection direction)
        {
            FDClimbEntry ladder = entries[0] as FDClimbEntry;
            Assert.IsFalse(ladder.Direction.HasFlag(direction));
            ladder.Direction = direction;

            level = WriteReadTempLevel(level);
            sector = level.Rooms[2].GetSector(4608, 6656);
            entries = level.FloorData[sector.FDIndex];

            ladder = entries[0] as FDClimbEntry;
            Assert.IsTrue(ladder.Direction.HasFlag(direction));

            Assert.AreEqual(ladder.IsPositiveX, direction.HasFlag(FDClimbDirection.PositiveX));
            Assert.AreEqual(ladder.IsPositiveZ, direction.HasFlag(FDClimbDirection.PositiveZ));
            Assert.AreEqual(ladder.IsNegativeX, direction.HasFlag(FDClimbDirection.NegativeX));
            Assert.AreEqual(ladder.IsNegativeZ, direction.HasFlag(FDClimbDirection.NegativeZ));
        }

        TestDirection(FDClimbDirection.PositiveX);
        TestDirection(FDClimbDirection.NegativeX);
        TestDirection(FDClimbDirection.PositiveZ);
        TestDirection(FDClimbDirection.NegativeZ);

        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.NegativeX);
        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.PositiveZ);
        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.NegativeZ);
        TestDirection(FDClimbDirection.NegativeX | FDClimbDirection.NegativeZ);
        TestDirection(FDClimbDirection.NegativeX | FDClimbDirection.PositiveZ);

        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.NegativeX | FDClimbDirection.PositiveZ);
        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.NegativeX | FDClimbDirection.NegativeZ);
        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.PositiveZ | FDClimbDirection.NegativeZ);
        TestDirection(FDClimbDirection.NegativeX | FDClimbDirection.PositiveZ | FDClimbDirection.NegativeZ);

        TestDirection(FDClimbDirection.PositiveX | FDClimbDirection.NegativeX | FDClimbDirection.PositiveZ | FDClimbDirection.NegativeZ);
    }

    [TestMethod]
    [Description("Add invalid FD entries for TR2.")]
    public void AddInvalidEntries()
    {
        TR2Level level = GetTR2TestLevel();

        TRRoomSector sector = level.Rooms[2].GetSector(4608, 6656);
        Assert.AreEqual(0, sector.FDIndex);

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].AddRange(new List<FDEntry>
        {
            new FDBeetleEntry(),
            new FDDeferredTriggerEntry(),
            new FDMinecartEntry(),
            new FDMonkeySwingEntry(),
            new FDTriangulationEntry(),
        });

        WriteReadTempLevel(level);
        Assert.AreEqual(0, sector.FDIndex);
    }
}
