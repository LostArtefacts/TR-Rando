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
        ExportTR1Model(TR1Type.Bear);

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
        ExportTR2Model(TR2Type.BengalTiger);

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
        ExportTR3Model(TR3Type.Monkey);

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
        ExportTR4Model(TR4Type.Dog);

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
        ExportTR5Model(TR5Type.Huskie);

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

    [TestMethod]
    [Description("Test that dependencies are calculated on import.")]
    public void TestDirectDependency()
    {
        ExportTR2Model(TR2Type.MaskedGoon1);
        ExportTR2Model(TR2Type.MaskedGoon2);

        TR2Level level = GetTR2AltTestLevel();
        TR2DataImporter importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.MaskedGoon2 },
        };

        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR2Type.MaskedGoon1));
        Assert.IsTrue(level.Models.ContainsKey(TR2Type.MaskedGoon2));
    }

    [TestMethod]
    [Description("Test that dependencies aren't removed until no longer required.")]
    public void TestDependencyRemoval()
    {
        ExportTR2Model(TR2Type.MaskedGoon1);
        ExportTR2Model(TR2Type.MaskedGoon2);
        ExportTR2Model(TR2Type.BengalTiger);

        TR2Level level = GetTR2AltTestLevel();
        TR2DataImporter importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.MaskedGoon1 },
        };

        importer.Import();

        importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.MaskedGoon2 },
            TypesToRemove = new() { TR2Type.MaskedGoon1 },
        };
        importer.Import();

        Assert.IsTrue(level.Models.ContainsKey(TR2Type.MaskedGoon1));

        importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.BengalTiger },
            TypesToRemove = new() { TR2Type.MaskedGoon1, TR2Type.MaskedGoon2 },
        };
        importer.Import();

        Assert.IsFalse(level.Models.ContainsKey(TR2Type.MaskedGoon1));
        Assert.IsFalse(level.Models.ContainsKey(TR2Type.MaskedGoon2));
    }

    private static void ExportTR1Model(TR1Type type)
    {
        TR1Level level = GetTR1TestLevel();
        TR1DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR1"
        };
        TR1Blob blob = exporter.Export(level, type);
        exporter.StoreBlob(blob);

        foreach (TR1Type dependency in blob.Dependencies)
        {
            ExportTR1Model(dependency);
        }
    }

    private static void ExportTR2Model(TR2Type type)
    {
        TR2Level level = GetTR2TestLevel();
        TR2DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR2"
        };
        TR2Blob blob = exporter.Export(level, type);
        exporter.StoreBlob(blob);

        foreach (TR2Type dependency in blob.Dependencies)
        {
            ExportTR2Model(dependency);
        }
    }

    private static void ExportTR3Model(TR3Type type)
    {
        TR3Level level = GetTR3TestLevel();
        TR3DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR3"
        };
        TR3Blob blob = exporter.Export(level, type);
        exporter.StoreBlob(blob);

        foreach (TR3Type dependency in blob.Dependencies)
        {
            ExportTR3Model(dependency);
        }
    }

    private static void ExportTR4Model(TR4Type type)
    {
        TR4Level level = GetTR4TestLevel();
        TR4DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR4"
        };
        TR4Blob blob = exporter.Export(level, type);
        exporter.StoreBlob(blob);

        foreach (TR4Type dependency in blob.Dependencies)
        {
            ExportTR4Model(dependency);
        }
    }

    private static void ExportTR5Model(TR5Type type)
    {
        TR5Level level = GetTR5TestLevel();
        TR5DataExporter exporter = new()
        {
            DataFolder = @"Objects\TR5"
        };
        TR5Blob blob = exporter.Export(level, type);
        exporter.StoreBlob(blob);

        foreach (TR5Type dependency in blob.Dependencies)
        {
            ExportTR5Model(dependency);
        }
    }
}
