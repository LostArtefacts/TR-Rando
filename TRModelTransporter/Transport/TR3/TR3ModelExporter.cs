using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport
{
    public class TR3ModelExporter : AbstractTRModelExporter<TR3Entities, TR3Level, TR3ModelDefinition>
    {
        public TR3ModelExporter()
        {
            Data = new TR3DefaultDataProvider();
        }

        protected override AbstractTextureExportHandler<TR3Entities, TR3Level, TR3ModelDefinition> CreateTextureHandler()
        {
            return new TR3TextureExportHandler();
        }

        protected override TR3ModelDefinition CreateModelDefinition(TR3Level level, TR3Entities modelEntity)
        {
            TR3ModelDefinition definition = new TR3ModelDefinition
            {
                Alias = modelEntity
            };

            if (Data.IsAlias(modelEntity))
            {
                modelEntity = Data.TranslateAlias(modelEntity);
            }

            _modelHandler.Export(level, definition, modelEntity);
            _meshHandler.Export(level, definition);
            _colourHandler.Export(level, definition);
            _textureHandler.Export(level, definition, TextureClassifier, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
            _animationHandler.Export(level, definition);
            _cinematicHandler.Export(level, definition, Data.GetCinematicEntities());
            _soundHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

            return definition;
        }
    }
}