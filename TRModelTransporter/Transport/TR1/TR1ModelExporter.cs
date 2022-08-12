using System;
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
    public class TR1ModelExporter : AbstractTRModelExporter<TREntities, TRLevel, TR1ModelDefinition>
    {
        public TR1ModelExporter()
        {
            Data = new TR1DefaultDataProvider();
        }

        protected override AbstractTextureExportHandler<TREntities, TRLevel, TR1ModelDefinition> CreateTextureHandler()
        {
            return new TR1TextureExportHandler();
        }

        protected override TR1ModelDefinition CreateModelDefinition(TRLevel level, TREntities modelEntity)
        {
            TR1ModelDefinition definition = new TR1ModelDefinition
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

        protected override void ModelExportReady(TR1ModelDefinition definition)
        {
            switch (definition.Entity)
            {
                case TREntities.Kold:
                    if (definition.Colours.ContainsKey(123))
                    {
                        // Incorrect orange colouring on head and hands
                        definition.Colours[123].Red = 28;
                        definition.Colours[123].Green = 18;
                        definition.Colours[123].Blue = 4;
                    }
                    break;
                case TREntities.SkateboardKid:
                    if (definition.Colours.ContainsKey(182))
                    {
                        // Incorrect yellow colouring on his arm
                        definition.Colours[182].Red = 51;
                        definition.Colours[182].Green = 33;
                        definition.Colours[182].Blue = 22;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}