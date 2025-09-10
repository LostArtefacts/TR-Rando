using TRGE.Core;
using TRRandomizerCore.Randomizers;

namespace TRRandomizerCoreTests;

[TestClass]
public class WaterColourTests
{
    [TestMethod]
    [DataRow(false, false, 0.10)] // normal
    [DataRow(false, true, 0.20)]  // night
    [DataRow(true, false, 0.30)]  // wireframe
    [DataRow(true, true, 0.40)]   // wireframe + night
    public void TestColourValues(bool isWireframe, bool isNightMode, double expectedMin)
    {
        var (level, allocator) = Setup();

        allocator.RandomizeWaterColour(level, isWireframe, isNightMode);

        Assert.IsNotNull(level.WaterColor);
        Assert.AreEqual(3, level.WaterColor.Length);

        foreach (var value in level.WaterColor)
        {
            Assert.IsTrue(value >= expectedMin && value <= 1.0);
        }
    }

    [TestMethod]
    public void TestCutscene()
    {
        var (level, allocator) = Setup();
        var cutscene = level.CutSceneLevel as TRXScriptedLevel;

        allocator.RandomizeWaterColour(level, false, false);

        Assert.IsNotNull(level.WaterColor);
        Assert.IsNotNull(cutscene.WaterColor);

        CollectionAssert.AreEqual(level.WaterColor, cutscene.WaterColor);
    }

    private static (TRXScriptedLevel, TRXTextureAllocator) Setup()
    {
        var level = new TRXScriptedLevel(TRVersion.TR1)
        {
            CutSceneLevel = new TRXScriptedLevel(TRVersion.TR1),
        };

        var allocator = new TRXTextureAllocator
        {
            Generator = new(),
            Settings = new()
            {
                RandomizeWaterColour = true,
            },
        };

        return (level, allocator);
    }
}
