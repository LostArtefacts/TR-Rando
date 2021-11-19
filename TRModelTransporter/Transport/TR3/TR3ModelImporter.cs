using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public class TR3ModelImporter : AbstractTRModelImporter<TR3Entities, TR3Level, TR3ModelDefinition>
    {
        public TR3ModelImporter()
        {
            Data = new TR3DefaultDataProvider();
        }

        protected override AbstractTextureImportHandler<TR3Entities, TR3Level, TR3ModelDefinition> CreateTextureHandler()
        {
            return new TR3TextureImportHandler();
        }

        protected override List<TR3Entities> GetExistingModelTypes()
        {
            List<TR3Entities> existingEntities = new List<TR3Entities>();
            Level.Models.ToList().ForEach(m => existingEntities.Add((TR3Entities)m.ID));
            return existingEntities;
        }

        protected override void Import(IEnumerable<TR3ModelDefinition> standardDefinitions, IEnumerable<TR3ModelDefinition> soundOnlyDefinitions)
        {
            TR3TextureRemapGroup remap = null;
            if (TextureRemapPath != null)
            {
                remap = JsonConvert.DeserializeObject<TR3TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
            }
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);

            _soundHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

            Dictionary<TR3Entities, TR3Entities> aliasPriority = Data.AliasPriority ?? new Dictionary<TR3Entities, TR3Entities>();

            foreach (TR3ModelDefinition definition in standardDefinitions)
            {
                _colourHandler.Import(Level, definition);
                _meshHandler.Import(Level, definition);
                _animationHandler.Import(Level, definition);
                _cinematicHandler.Import(Level, definition);
                _modelHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants());
            }

            _textureHandler.ResetUnusedTextures();
        }
    }
}