using TRDataControl.Environment;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.Environment.Entities;

[TestClass]
[TestCategory("Environment")]
public class LadderTests : TestBase
{
    [TestMethod]
    public void TestNewLadder()
    {
        var (level, location, sector) = Setup();
        Assert.AreEqual(0, sector.FDIndex);

        new EMLadderFunction
        {
            TextureMap = [],
            Location = location,
            IsNegativeX = true,
            IsPositiveZ = true,
        }.ApplyToLevel(level);

        Assert.AreNotEqual(0, sector.FDIndex);
        var ladder = level.FloorData[sector.FDIndex].OfType<FDClimbEntry>().FirstOrDefault();
        Assert.IsNotNull(ladder);

        Assert.IsTrue(ladder.IsNegativeX);
        Assert.IsTrue(ladder.IsPositiveZ);
        Assert.IsFalse(ladder.IsPositiveX);
        Assert.IsFalse(ladder.IsNegativeZ);
    }

    [TestMethod]
    public void TestAlteredLadder()
    {
        var (level, location, sector) = Setup();

        new EMLadderFunction
        {
            TextureMap = [],
            Location = location,
            IsNegativeX = true,
            IsPositiveZ = true,
        }.ApplyToLevel(level);

        new EMLadderFunction
        {
            TextureMap = [],
            Location = location,
            IsPositiveX = true,
            IsNegativeZ = true,
        }.ApplyToLevel(level);

        var ladders = level.FloorData[sector.FDIndex].OfType<FDClimbEntry>();
        Assert.AreEqual(1, ladders.Count());

        var ladder = ladders.First();
        Assert.IsFalse(ladder.IsNegativeX);
        Assert.IsFalse(ladder.IsPositiveZ);
        Assert.IsTrue(ladder.IsPositiveX);
        Assert.IsTrue(ladder.IsNegativeZ);
    }

    [TestMethod]
    public void TestRemovedLadder()
    {
        var (level, location, sector) = Setup();

        new EMLadderFunction
        {
            TextureMap = [],
            Location = location,
            IsNegativeX = true,
            IsPositiveZ = true,
        }.ApplyToLevel(level);

        var ladder = level.FloorData[sector.FDIndex].OfType<FDClimbEntry>().FirstOrDefault();
        Assert.IsNotNull(ladder);

        new EMLadderFunction
        {
            TextureMap = [],
            Location = location,
        }.ApplyToLevel(level);

        ladder = level.FloorData[sector.FDIndex].OfType<FDClimbEntry>().FirstOrDefault();
        Assert.IsNull(ladder);
    }

    [TestMethod]
    public void TestMultipleLadders()
    {
        var (level, location, sector) = Setup();

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].Add(new FDClimbEntry { IsPositiveX = true });
        level.FloorData[sector.FDIndex].Add(new FDClimbEntry { IsNegativeZ = true });

        level = WriteReadTempLevel(level);
        sector = level.GetRoomSector(location);

        var ladders = level.FloorData[sector.FDIndex].OfType<FDClimbEntry>();
        Assert.AreEqual(1, ladders.Count());

        var ladder = ladders.First();
        Assert.IsTrue(ladder.IsPositiveX);
        Assert.IsTrue(ladder.IsNegativeZ);
        Assert.IsFalse(ladder.IsNegativeX);
        Assert.IsFalse(ladder.IsPositiveZ);
    }

    [TestMethod]
    public void TestNullLadder()
    {
        var (level, location, sector) = Setup();

        level.FloorData.CreateFloorData(sector);
        level.FloorData[sector.FDIndex].Add(new FDClimbEntry());

        level = WriteReadTempLevel(level);
        sector = level.GetRoomSector(location);
        Assert.AreEqual(0, sector.FDIndex);
    }

    private static (TR2Level, EMLocation, TRRoomSector) Setup()
    {
        var level = GetTR2TestLevel();
        var location = new EMLocation
        {
            X = 3584,
            Z = 10752,
        };
        return (level, location, level.GetRoomSector(location));
    }
}
