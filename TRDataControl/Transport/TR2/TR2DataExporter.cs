using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR2DataExporter : TRDataExporter<TR2Type, TR2Level, TR2Blob>
{
    public TR2DataExporter()
    {
        Data = new TR2DataProvider();
    }

    protected override AbstractTextureExportHandler<TR2Type, TR2Level, TR2Blob> CreateTextureHandler()
    {
        return new TR2TextureExportHandler();
    }

    protected override TR2Blob CreateModelDefinition(TR2Level level, TR2Type modelEntity)
    {
        TR2Blob definition = new()
        {
            Alias = modelEntity
        };

        if (Data.IsAlias(modelEntity))
        {
            modelEntity = Data.TranslateAlias(modelEntity);
        }

        ModelTransportHandler.Export(level, definition, modelEntity);
        ColourTransportHandler.Export(level, definition);
        _textureHandler.Export(level, definition, TextureClassifier, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
        CinematicTransportHandler.Export(level, definition, Data.GetCinematicEntities());
        SoundTransportHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

        return definition;
    }

    protected override void ModelExportReady(TR2Blob definition)
    {
        switch (definition.Alias)
        {
            case TR2Type.FlamethrowerGoonTopixtor:
                AmendDXtre3DTextures(definition);
                AmendDXtre3DFlameTextures(definition);
                break;
            case TR2Type.Gunman1TopixtorORC:
            case TR2Type.Gunman1TopixtorCAC:
                AmendDXtre3DTextures(definition);
                break;
            default:
                break;
        }
    }

    private static void AmendDXtre3DFlameTextures(TR2Blob definition)
    {
        if (!definition.SpriteSequences.ContainsKey(TR2Type.Flame_S_H))
        {
            return;
        }

        // Ensures the flame sprite is aligned to OG - required for texture monitoring
        Dictionary<int, List<IndexedTRSpriteTexture>> defaultSprites = definition.SpriteTextures[TR2Type.Flame_S_H];
        foreach (int id in defaultSprites.Keys)
        {
            foreach (IndexedTRSpriteTexture sprite in defaultSprites[id])
            {
                sprite.Index += 22;
            }
        }
    }
}
