using TRDataControl.Environment;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.Environment.Triggers;

[TestClass]
public class MergeTests : TestBase
{
    [TestMethod]
    public void TestMerge()
    {
        var level = GetTR1TestLevel();
        var baseLoc = new EMLocation
        {
            X = 15872,
            Z = 20992,
            Room = 7,
        };
        var targetLoc = new EMLocation
        {
            X = 5632,
            Z = 20992,
        };

        var sectorA = level.GetRoomSector(baseLoc);
        var triggerA = level.FloorData[sectorA.FDIndex].OfType<FDTriggerEntry>().FirstOrDefault();
        Assert.IsTrue(triggerA.OneShot);
        Assert.HasCount(1, triggerA.Actions.Where(a => a.Parameter == 13));

        var sectorB = level.GetRoomSector(targetLoc);
        var triggerB = level.FloorData[sectorB.FDIndex].OfType<FDTriggerEntry>().FirstOrDefault();
        Assert.IsFalse(triggerB.OneShot);
        Assert.HasCount(0, triggerB.Actions.Where(a => a.Parameter == 13));

        new EMMergeTriggersFunction
        {
            BaseLocation = baseLoc,
            TargetLocation = targetLoc,
        }.ApplyToLevel(level);

        Assert.IsTrue(triggerB.OneShot);
        Assert.HasCount(1, triggerB.Actions.Where(a => a.Parameter == 13));

        Assert.HasCount(0, level.FloorData[sectorA.FDIndex].OfType<FDTriggerEntry>());
    }
}
