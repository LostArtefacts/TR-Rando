using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Transport;

namespace TREnvironmentEditor.Model.Types;

public class EMImportModelFunction : BaseEMFunction
{
    public List<short> Models { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TR1ModelImporter importer = new TR1ModelImporter(Tags?.Contains(EMTag.CommunityPatchOnly) ?? false)
        {
            Level = level,
            EntitiesToImport = Models.Select(m => (TREntities)m),
            DataFolder = @"Resources\TR1\Models"
        };

        importer.Import();
    }

    public override void ApplyToLevel(TR2Level level)
    {
        TR2ModelImporter importer = new TR2ModelImporter
        {
            Level = level,
            EntitiesToImport = Models.Select(m => (TR2Entities)m),
            DataFolder = @"Resources\TR2\Models"
        };

        importer.Import();
    }

    public override void ApplyToLevel(TR3Level level)
    {
        TR3ModelImporter importer = new TR3ModelImporter
        {
            Level = level,
            EntitiesToImport = Models.Select(m => (TR3Entities)m),
            DataFolder = @"Resources\TR3\Models"
        };

        importer.Import();
    }
}
