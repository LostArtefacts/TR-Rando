using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Model;

namespace TRModelTransporter.Transport
{
    public class MassTREnemyExporter
    {
        private readonly TRModelBuilder _exporter;

        public MassTREnemyExporter()
        {
            _exporter = new TRModelBuilder();
        }

        public void Export(string levelFileDirectory, string outputDirectory)
        {
            List<TR2Entities> processedEntities = new List<TR2Entities>();
            Dictionary<string, List<TR2Entities>> enemies = TR2EntityUtilities.GetEnemyTypeDictionary();

            TR2LevelReader reader = new TR2LevelReader();
            foreach (string lvlName in LevelNames.AsList)
            {
                TR2Level level = reader.ReadLevel(Path.Combine(levelFileDirectory, lvlName));
                foreach (TR2Entities entity in enemies[lvlName])
                {
                    if (!processedEntities.Contains(entity))
                    {
                        Export(level, outputDirectory, entity);
                        processedEntities.Add(entity);
                    }
                }

                if (lvlName == LevelNames.TIBET)
                {
                    Export(level, outputDirectory, TR2Entities.BlackSnowmob);
                }
            }
        }

        public void Export(TR2Level level, string outputDirectory, TR2Entities entity)
        {
            _exporter.Level = level;

            TRModelDefinition definition = _exporter.CreateModelDefinition(entity);

            string directory = Path.Combine(outputDirectory, entity.ToString());
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            definition.Bitmap.Save(Path.Combine(directory, "Segments.png"), ImageFormat.Png);
            File.WriteAllText(Path.Combine(directory, "Data.json"), JsonConvert.SerializeObject(definition, Formatting.Indented));
        }
    }
}