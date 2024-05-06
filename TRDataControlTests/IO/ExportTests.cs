using Newtonsoft.Json;
using TRDataControl;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.IO;

[TestClass]
[TestCategory("DataTransport")]
public class ExportTests : TestBase
{
    [TestMethod]
    [Description("Test creating a TR1 model export.")]
    public void TestTR1ExportProperties()
    {
        TR1Level level = GetTR1TestLevel();
        TR1DataExporter exporter = new();
        TR1Blob blob = exporter.Export(level, TR1Type.Bear, TRBlobType.Model);

        Assert.AreEqual(TR1Type.Bear, blob.ID);
        Assert.AreEqual(TR1Type.Bear, blob.Alias);
        Assert.AreEqual(TRBlobType.Model, blob.Type);
        Assert.IsFalse(blob.IsDependencyOnly);
        Assert.AreEqual(0, blob.Dependencies.Count);
        Assert.AreEqual(level.Models[TR1Type.Bear], blob.Model);
        Assert.IsNull(blob.CinematicFrames);

        Assert.AreEqual(6, blob.SoundEffects.Count);
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearGrowl));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearFeet));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearAttack));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearSnarl));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearHurt));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR1SFX.BearDeath));
    }

    [TestMethod]
    [Description("Test TR1 model export IO.")]
    public void TestTR1ExportIO()
    {
        TR1Level level = GetTR1TestLevel();
        TR1DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR1"
        };
        TR1Blob blob1 = exporter.Export(level, TR1Type.Bear, TRBlobType.Model);
        exporter.StoreBlob(blob1);

        TR1Blob blob2 = exporter.LoadBlob(TR1Type.Bear);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }

    [TestMethod]
    [Description("Test creating a TR2 model export.")]
    public void TestTR2ExportProperties()
    {
        TR2Level level = GetTR2TestLevel();
        TR2DataExporter exporter = new();
        TR2Blob blob = exporter.Export(level, TR2Type.BengalTiger, TRBlobType.Model);

        Assert.AreEqual(TR2Type.TigerOrSnowLeopard, blob.ID);
        Assert.AreEqual(TR2Type.BengalTiger, blob.Alias);
        Assert.AreEqual(TRBlobType.Model, blob.Type);
        Assert.IsFalse(blob.IsDependencyOnly);
        Assert.AreEqual(0, blob.Dependencies.Count);
        Assert.AreEqual(level.Models[TR2Type.TigerOrSnowLeopard], blob.Model);
        Assert.IsNull(blob.CinematicFrames);

        Assert.AreEqual(9, blob.SoundEffects.Count);
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.LaraWetFeet));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.LaraSplash));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.BodySlam));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.LeopardFeet));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.TigerRoar));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.TigerBite));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.TigerStrike));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.TigerDeath));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR2SFX.TigerGrowl));
    }

    [TestMethod]
    [Description("Test TR2 model export IO.")]
    public void TestTR2ExportIO()
    {
        TR2Level level = GetTR2TestLevel();
        TR2DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR2"
        };
        TR2Blob blob1 = exporter.Export(level, TR2Type.BengalTiger, TRBlobType.Model);
        exporter.StoreBlob(blob1);

        TR2Blob blob2 = exporter.LoadBlob(TR2Type.BengalTiger);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }

    [TestMethod]
    [Description("Test creating a TR3 model export.")]
    public void TestTR3ExportProperties()
    {
        TR3Level level = GetTR3TestLevel();
        TR3DataExporter exporter = new();
        TR3Blob blob = exporter.Export(level, TR3Type.Monkey, TRBlobType.Model);

        Assert.AreEqual(TR3Type.Monkey, blob.ID);
        Assert.AreEqual(TR3Type.Monkey, blob.Alias);
        Assert.AreEqual(TRBlobType.Model, blob.Type);
        Assert.IsFalse(blob.IsDependencyOnly);
        Assert.AreEqual(2, blob.Dependencies.Count);
        Assert.IsTrue(blob.Dependencies.Contains(TR3Type.MonkeyKeyMeshswap));
        Assert.IsTrue(blob.Dependencies.Contains(TR3Type.MonkeyMedMeshswap));
        Assert.AreEqual(level.Models[TR3Type.Monkey], blob.Model);
        Assert.IsNull(blob.CinematicFrames);

        Assert.AreEqual(7, blob.SoundEffects.Count);
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyStandWait));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyAttackLow));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyAttackJump));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyJump));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyDeath));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyChatter));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR3SFX.MonkeyRoll));
    }

    [TestMethod]
    [Description("Test TR3 model export IO.")]
    public void TestTR3ExportIO()
    {
        TR3Level level = GetTR3TestLevel();
        TR3DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR3"
        };
        TR3Blob blob1 = exporter.Export(level, TR3Type.Monkey, TRBlobType.Model);
        exporter.StoreBlob(blob1);

        TR3Blob blob2 = exporter.LoadBlob(TR3Type.Monkey);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }
}
