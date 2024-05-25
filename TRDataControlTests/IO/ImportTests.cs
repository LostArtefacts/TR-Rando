using TRDataControl;
using TRLevelControl;
using TRLevelControl.Helpers;
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
    [Description("Test merging TR1R data.")]
    public void TestTR1RMerge()
    {
        ExportTR1Model(TR1Type.Bear);
        ExportTR1RPDP(TR1LevelNames.CAVES);

        TR1Level level = GetTR1AltTestLevel();
        level.Models[TR1Type.Larson] = new();

        TR1DataImporter importer = new()
        {
            DataFolder = @"Objects\TR1",
            Level = level,
            TypesToImport = new() { TR1Type.Bear },
            TypesToRemove = new() { TR1Type.Larson },
        };
        ImportResult<TR1Type> result = importer.Import();

        Assert.IsTrue(result.ImportedTypes.Contains(TR1Type.Bear));
        Assert.IsTrue(result.RemovedTypes.Contains(TR1Type.Larson));

        TRDictionary<TR1Type, TRModel> pdpData = new()
        {
            [TR1Type.Larson] = new(),
        };
        Dictionary<TR1Type, TR1RAlias> mapData = new()
        {
            [TR1Type.Larson] = TR1RAlias.LARSON_EGYPT
        };

        TR1RDataCache dataCache = new()
        {
            PDPFolder = "PDP"
        };
        dataCache.Merge(result, pdpData, mapData);

        Assert.IsTrue(pdpData.ContainsKey(TR1Type.Bear));
        Assert.IsFalse(pdpData.ContainsKey(TR1Type.Larson));
        Assert.IsFalse(mapData.ContainsKey(TR1Type.Larson));
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
    [Description("Test merging TR2R data.")]
    public void TestTR2RMerge()
    {
        ExportTR2Model(TR2Type.BengalTiger);
        ExportTR2RPDP(TR2LevelNames.GW);

        TR2Level level = GetTR2AltTestLevel();
        level.Models[TR2Type.Yeti] = new();

        TR2DataImporter importer = new()
        {
            DataFolder = @"Objects\TR2",
            Level = level,
            TypesToImport = new() { TR2Type.BengalTiger },
            TypesToRemove = new() { TR2Type.Yeti },
        };
        ImportResult<TR2Type> result = importer.Import();

        Assert.IsTrue(result.ImportedTypes.Contains(TR2Type.BengalTiger));
        Assert.IsTrue(result.RemovedTypes.Contains(TR2Type.Yeti));

        TRDictionary<TR2Type, TRModel> pdpData = new()
        {
            [TR2Type.Yeti] = new(),
        };
        Dictionary<TR2Type, TR2RAlias> mapData = new()
        {
            [TR2Type.Yeti] = TR2RAlias.BANDIT2B_1
        };

        TR2RDataCache dataCache = new()
        {
            PDPFolder = "PDP"
        };
        dataCache.Merge(result, pdpData, mapData);

        Assert.IsTrue(pdpData.ContainsKey(TR2Type.TigerOrSnowLeopard));
        Assert.IsFalse(pdpData.ContainsKey(TR2Type.Yeti));
        Assert.IsFalse(mapData.ContainsKey(TR2Type.Yeti));
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
    [Description("Test merging TR3R data.")]
    public void TestTR3RMerge()
    {
        ExportTR3Model(TR3Type.Monkey);
        ExportTR3RPDP(TR3LevelNames.JUNGLE);

        TR3Level level = GetTR3AltTestLevel();
        level.Models[TR3Type.Dog] = new();

        TR3DataImporter importer = new()
        {
            DataFolder = @"Objects\TR3",
            Level = level,
            TypesToImport = new() { TR3Type.Monkey },
            TypesToRemove = new() { TR3Type.Dog },
        };
        ImportResult<TR3Type> result = importer.Import();

        Assert.IsTrue(result.ImportedTypes.Contains(TR3Type.Monkey));
        Assert.IsTrue(result.RemovedTypes.Contains(TR3Type.Dog));

        TRDictionary<TR3Type, TRModel> pdpData = new()
        {
            [TR3Type.Dog] = new(),
        };
        Dictionary<TR3Type, TR3RAlias> mapData = new()
        {
            [TR3Type.Dog] = TR3RAlias.DOG_SEWER
        };

        TR3RDataCache dataCache = new()
        {
            PDPFolder = "PDP"
        };
        dataCache.Merge(result, pdpData, mapData);

        Assert.IsTrue(pdpData.ContainsKey(TR3Type.Monkey));
        Assert.IsFalse(pdpData.ContainsKey(TR3Type.Dog));
        Assert.IsFalse(mapData.ContainsKey(TR3Type.Dog));
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

    private static void ExportTR1RPDP(string levelName)
    {
        TR1Level level = GetTR1TestLevel();
        TR1PDPControl control = new();
        Directory.CreateDirectory("PDP");
        control.Write(level.Models, Path.Combine("PDP", Path.GetFileNameWithoutExtension(levelName) + ".PDP"));
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

    private static void ExportTR2RPDP(string levelName)
    {
        TR2Level level = GetTR2TestLevel();
        TR2PDPControl control = new();
        Directory.CreateDirectory("PDP");
        control.Write(level.Models, Path.Combine("PDP", Path.GetFileNameWithoutExtension(levelName) + ".PDP"));
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

    private static void ExportTR3RPDP(string levelName)
    {
        TR3Level level = GetTR3TestLevel();
        TR3PDPControl control = new();
        Directory.CreateDirectory("PDP");
        control.Write(level.Models, Path.Combine("PDP", Path.GetFileNameWithoutExtension(levelName) + ".PDP"));
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
