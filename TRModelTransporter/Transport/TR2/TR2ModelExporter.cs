using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport
{
    public class TR2ModelExporter : AbstractTRModelExporter<TR2Entities, TR2Level, TR2ModelDefinition>
    {
        public TR2ModelExporter()
        {
            Data = new TR2DefaultDataProvider();
        }

        protected override AbstractTextureExportHandler<TR2Entities, TR2Level, TR2ModelDefinition> CreateTextureHandler()
        {
            return new TR2TextureExportHandler();
        }

        protected override TR2ModelDefinition CreateModelDefinition(TR2Level level, TR2Entities modelEntity)
        {
            TR2ModelDefinition definition = new TR2ModelDefinition
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