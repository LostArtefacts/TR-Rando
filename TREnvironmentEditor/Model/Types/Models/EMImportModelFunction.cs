using System.Collections.Generic;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;

namespace TREnvironmentEditor.Model.Types
{
    public class EMImportModelFunction : BaseEMFunction
    {
        public List<short> Models { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            List<TREntities> types = new List<TREntities>();
            Models.ForEach(m => types.Add((TREntities)m));

            TR1ModelImporter importer = new TR1ModelImporter
            {
                Level = level,
                EntitiesToImport = types,
                DataFolder = @"Resources\TR1\Models"
            };

            importer.Import();
        }

        public override void ApplyToLevel(TR2Level level)
        {
            List<TR2Entities> types = new List<TR2Entities>();
            Models.ForEach(m => types.Add((TR2Entities)m));

            TR2ModelImporter importer = new TR2ModelImporter
            {
                Level = level,
                EntitiesToImport = types,
                DataFolder = @"Resources\TR2\Models"
            };

            importer.Import();
        }

        public override void ApplyToLevel(TR3Level level)
        {
            List<TR3Entities> types = new List<TR3Entities>();
            Models.ForEach(m => types.Add((TR3Entities)m));

            TR3ModelImporter importer = new TR3ModelImporter
            {
                Level = level,
                EntitiesToImport = types,
                DataFolder = @"Resources\TR3\Models"
            };

            importer.Import();
        }
    }
}