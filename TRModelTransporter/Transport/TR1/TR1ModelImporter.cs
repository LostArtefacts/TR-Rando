using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport
{
    public class TR1ModelImporter : AbstractTRModelImporter<TREntities, TRLevel, TR1ModelDefinition>
    {
        public TR1ModelImporter()
        {
            Data = new TR1DefaultDataProvider();
        }

        protected override AbstractTextureImportHandler<TREntities, TRLevel, TR1ModelDefinition> CreateTextureHandler()
        {
            return new TR1TextureImportHandler();
        }

        protected override List<TREntities> GetExistingModelTypes()
        {
            List<TREntities> existingEntities = new List<TREntities>();
            Level.Models.ToList().ForEach(m => existingEntities.Add((TREntities)m.ID));
            return existingEntities;
        }

        protected override void Import(IEnumerable<TR1ModelDefinition> standardDefinitions, IEnumerable<TR1ModelDefinition> soundOnlyDefinitions)
        {
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, null, ClearUnusedSprites, TexturePositionMonitor);

            _soundHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

            Dictionary<TREntities, TREntities> aliasPriority = Data.AliasPriority ?? new Dictionary<TREntities, TREntities>();

            foreach (TR1ModelDefinition definition in standardDefinitions)
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