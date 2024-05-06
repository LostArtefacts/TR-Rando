using TRDataControl;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.IO;

[TestClass]
[TestCategory("DataTransport")]
public class ImportTests : TestBase
{
    [TestMethod]
    [Description("Test importing a TR1 model.")]
    public void TestTR1Import()
    {
        ExportTR1Model(TR1Type.Bear, TRBlobType.Model);

        TR1Level level = GetTR1AltTestLevel();
        Assert.IsFalse(level.Models.ContainsKey(TR1Type.Bear));

        TR1DataImporter importer = new()
        {
            DataFolder = @"Objects\TR1",
            Level = level,
            TypesToImport = new() { TR1Type.Bear },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR1Type.Bear));
    }

    [TestMethod]
    [Description("Test importing a TR2 model.")]
    public void TestTR2Import()
    {
        ExportTR2Model(TR2Type.BengalTiger, TRBlobType.Model);

        TR2Level level = GetTR2AltTestLevel();
        Assert.IsFalse(level.Models.ContainsKey(TR2Type.TigerOrSnowLeopard));

        TR2DataImporter importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.BengalTiger },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR2Type.TigerOrSnowLeopard));
    }

    [TestMethod]
    [Description("Test importing a TR3 model.")]
    public void TestTR3Import()
    {
        ExportTR3Model(TR3Type.Monkey, TRBlobType.Model);

        TR3Level level = GetTR3AltTestLevel();
        Assert.IsFalse(level.Models.ContainsKey(TR3Type.Monkey));

        TR3DataImporter importer = new()
        {
            DataFolder = @"Objects\TR3",
            Level = level,
            TypesToImport = new() { TR3Type.Monkey },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR3Type.Monkey));
    }

    [TestMethod]
    [Description("Test importing a TR4 model.")]
    public void TestTR4Import()
    {
        ExportTR4Model(TR4Type.Dog, TRBlobType.Model);

        TR4Level level = GetTR4AltTestLevel();
        Assert.IsFalse(level.Models.ContainsKey(TR4Type.Dog));

        TR4DataImporter importer = new()
        {
            DataFolder = @"Objects\TR4",
            Level = level,
            TypesToImport = new() { TR4Type.Dog },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR4Type.Dog));
    }

    [TestMethod]
    [Description("Test importing a TR5 model.")]
    public void TestTR5Import()
    {
        ExportTR5Model(TR5Type.Huskie, TRBlobType.Model);

        TR5Level level = GetTR5AltTestLevel();
        Assert.IsFalse(level.Models.ContainsKey(TR5Type.Huskie));

        TR5DataImporter importer = new()
        {
            DataFolder = @"Objects\TR5",
            Level = level,
            TypesToImport = new() { TR5Type.Huskie },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR5Type.Huskie));
    }

    [TestMethod]
    [Description("Test that importing a non-specific type that has aliases fails.")]
    public void TestAliasImport()
    {
        TR2DataImporter importer = new()
        {
            Level = GetTR2TestLevel(),
            TypesToImport = new() { TR2Type.StickWieldingGoon1 },
        };

        try
        {
            importer.Import();
            Assert.Fail();
        }
        catch (TransportException) { }
    }

    private static void ExportTR1Model(TR1Type type, TRBlobType blobType)
    {
        TR1Level level = GetTR1TestLevel();
        TR1DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR1"
        };
        TR1Blob blob = exporter.Export(level, type, blobType);
        exporter.StoreBlob(blob);

        foreach (TR1Type dependency in blob.Dependencies)
        {
            ExportTR1Model(dependency, exporter.Data.GetBlobType(dependency));
        }
    }

    private static void ExportTR2Model(TR2Type type, TRBlobType blobType)
    {
        TR2Level level = GetTR2TestLevel();
        TR2DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR2"
        };
        TR2Blob blob = exporter.Export(level, type, blobType);
        exporter.StoreBlob(blob);

        foreach (TR2Type dependency in blob.Dependencies)
        {
            ExportTR2Model(dependency, exporter.Data.GetBlobType(dependency));
        }
    }

    private static void ExportTR3Model(TR3Type type, TRBlobType blobType)
    {
        TR3Level level = GetTR3TestLevel();
        TR3DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR3"
        };
        TR3Blob blob = exporter.Export(level, type, blobType);
        exporter.StoreBlob(blob);

        foreach (TR3Type dependency in blob.Dependencies)
        {
            ExportTR3Model(dependency, exporter.Data.GetBlobType(dependency));
        }
    }

    private static void ExportTR4Model(TR4Type type, TRBlobType blobType)
    {
        TR4Level level = GetTR4TestLevel();
        TR4DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR4"
        };
        TR4Blob blob = exporter.Export(level, type, blobType);
        exporter.StoreBlob(blob);

        foreach (TR4Type dependency in blob.Dependencies)
        {
            ExportTR4Model(dependency, exporter.Data.GetBlobType(dependency));
        }
    }

    private static void ExportTR5Model(TR5Type type, TRBlobType blobType)
    {
        TR5Level level = GetTR5TestLevel();
        TR5DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR5"
        };
        TR5Blob blob = exporter.Export(level, type, blobType);
        exporter.StoreBlob(blob);

        foreach (TR5Type dependency in blob.Dependencies)
        {
            ExportTR5Model(dependency, exporter.Data.GetBlobType(dependency));
        }
    }
}
