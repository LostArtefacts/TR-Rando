using TRLevelControl.Model;

namespace TRLevelControlTests.TR2;

[TestClass]
[TestCategory("Sound")]
public class SoundTests : TestBase
{
    [TestMethod]
    [Description("Test sample ID order.")]
    public void TestSampleOrder()
    {
        var level = GetTR2TestLevel();

        var feet = level.SoundEffects[TR2SFX.LaraFeet];
        var breath = level.SoundEffects[TR2SFX.LaraBreath];
        Assert.IsGreaterThan(feet.SampleID, breath.SampleID);

        (breath.SampleID, feet.SampleID) = (feet.SampleID, breath.SampleID);

        try
        {
            WriteReadTempLevel(level);
        }
        catch
        {
            Assert.Fail();
        }
    }
}
