using TRLevelControl.Model;
using TRLevelControl.Model.TRX;

namespace TRLevelControlTests.TRX;

[TestClass]
[TestCategory("TRXInjection")]
public class IOTests : TestBase
{
    [TestMethod]
    public void TestSFX()
    {
        var level = GetTR1TestLevel();
        Assert.IsNull(level.TRXData);

        var sfxA = new TRSFXData
        {
            ID = (short)TR1SFX.NatlaDeath,
            Chance = 4,
            Flags = 1 << 2,
            Volume = 16384,
            Data = [[.. Enumerable.Range(0, 256).Select(i => (byte)i)]],
        };
        level.TRXData = new();
        level.TRXData.SFX.Add(sfxA);
        level.TRXData.SFX.Add(sfxA);

        level = WriteReadTempLevel(level);

        Assert.IsNotNull(level.TRXData);
        Assert.HasCount(2, level.TRXData.SFX);
        Assert.AreNotEqual(level.TRXData.SFX[0], level.TRXData.SFX[1]);

        for (int i = 0; i < 2; i++)
        {
            var sfxB = level.TRXData.SFX[i];
            Assert.AreEqual(sfxA.ID, sfxB.ID);
            Assert.AreEqual(sfxA.Chance, sfxB.Chance);
            Assert.AreEqual(sfxA.Flags, sfxB.Flags);
            Assert.AreEqual(sfxA.Volume, sfxB.Volume);
            CollectionAssert.AreEqual(sfxA.Data, sfxB.Data);
        }
    }
}
