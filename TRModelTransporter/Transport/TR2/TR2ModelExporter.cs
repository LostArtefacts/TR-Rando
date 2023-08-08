using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport;

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
        TR2ModelDefinition definition = new()
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

    protected override void ModelExportReady(TR2ModelDefinition definition)
    {
        switch (definition.Alias)
        {
            case TR2Entities.FlamethrowerGoonTopixtor:
                AmendDXtre3DTextures(definition);
                AmendDXtre3DFlameTextures(definition);
                break;
            case TR2Entities.Gunman1TopixtorORC:
            case TR2Entities.Gunman1TopixtorCAC:
                AmendDXtre3DTextures(definition);
                break;
            default:
                break;
        }
    }

    private void AmendDXtre3DFlameTextures(TR2ModelDefinition definition)
    {
        if (!definition.SpriteSequences.ContainsKey(TR2Entities.Flame_S_H))
        {
            return;
        }

        // Ensures the flame sprite is aligned to OG - required for texture monitoring
        TRSpriteSequence seq = definition.SpriteSequences[TR2Entities.Flame_S_H];
        seq.Offset += 22;

        Dictionary<int, List<IndexedTRSpriteTexture>> defaultSprites = definition.SpriteTextures[TR2Entities.Flame_S_H];
        foreach (int id in defaultSprites.Keys)
        {
            foreach (IndexedTRSpriteTexture sprite in defaultSprites[id])
            {
                sprite.Index += 22;
            }
        }
    }
}
