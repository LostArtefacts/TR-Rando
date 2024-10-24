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
        TR1Blob blob = exporter.Export(level, TR1Type.Bear);

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
            DataFolder = "Objects/TR1"
        };
        TR1Blob blob1 = exporter.Export(level, TR1Type.Bear);
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
        TR2Blob blob = exporter.Export(level, TR2Type.BengalTiger);

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
            DataFolder = "Objects/TR2"
        };
        TR2Blob blob1 = exporter.Export(level, TR2Type.BengalTiger);
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
        TR3Blob blob = exporter.Export(level, TR3Type.Monkey);

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
            DataFolder = "Objects/TR3"
        };
        TR3Blob blob1 = exporter.Export(level, TR3Type.Monkey);
        exporter.StoreBlob(blob1);

        TR3Blob blob2 = exporter.LoadBlob(TR3Type.Monkey);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }

    [TestMethod]
    [Description("Test creating a TR4 model export.")]
    public void TestTR4ExportProperties()
    {
        TR4Level level = GetTR4TestLevel();
        TR4DataExporter exporter = new();
        TR4Blob blob = exporter.Export(level, TR4Type.Dog);

        Assert.AreEqual(TR4Type.Dog, blob.ID);
        Assert.AreEqual(TR4Type.Dog, blob.Alias);
        Assert.AreEqual(TRBlobType.Model, blob.Type);
        Assert.IsFalse(blob.IsDependencyOnly);
        Assert.AreEqual(0, blob.Dependencies.Count);
        Assert.AreEqual(level.Models[TR4Type.Dog], blob.Model);
        Assert.IsNull(blob.CinematicFrames);

        Assert.AreEqual(3, blob.SoundEffects.Count);
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR4SFX.DogHowl));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR4SFX.DogDeath));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR4SFX.DogBite));
    }

    [TestMethod]
    [Description("Test TR4 model export IO.")]
    public void TestTR4ExportIO()
    {
        TR4Level level = GetTR4TestLevel();
        TR4DataExporter exporter = new()
        {
            DataFolder = "Objects/TR4"
        };
        TR4Blob blob1 = exporter.Export(level, TR4Type.Dog);
        exporter.StoreBlob(blob1);

        TR4Blob blob2 = exporter.LoadBlob(TR4Type.Dog);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }

    [TestMethod]
    [Description("Test creating a TR5 model export.")]
    public void TestTR5ExportProperties()
    {
        TR5Level level = GetTR5TestLevel();
        TR5DataExporter exporter = new();
        TR5Blob blob = exporter.Export(level, TR5Type.Huskie);

        Assert.AreEqual(TR5Type.Huskie, blob.ID);
        Assert.AreEqual(TR5Type.Huskie, blob.Alias);
        Assert.AreEqual(TRBlobType.Model, blob.Type);
        Assert.IsFalse(blob.IsDependencyOnly);
        Assert.AreEqual(0, blob.Dependencies.Count);
        Assert.AreEqual(level.Models[TR5Type.Huskie], blob.Model);
        Assert.IsNull(blob.CinematicFrames);

        Assert.AreEqual(4, blob.SoundEffects.Count);
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR5SFX.LaraSplash));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR5SFX.DogHowl));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR5SFX.DogDeath));
        Assert.IsTrue(blob.SoundEffects.ContainsKey(TR5SFX.DogAttack1));
    }

    [TestMethod]
    [Description("Test TR5 model export IO.")]
    public void TestTR5ExportIO()
    {
        TR5Level level = GetTR5TestLevel();
        TR5DataExporter exporter = new()
        {
            DataFolder = "Objects/TR5"
        };
        TR5Blob blob1 = exporter.Export(level, TR5Type.Huskie);
        exporter.StoreBlob(blob1);

        TR5Blob blob2 = exporter.LoadBlob(TR5Type.Huskie);

        string json1 = JsonConvert.SerializeObject(blob1);
        string json2 = JsonConvert.SerializeObject(blob2);

        Assert.AreEqual(json1, json2);
    }
}
