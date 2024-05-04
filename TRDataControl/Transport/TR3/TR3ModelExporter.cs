using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR3ModelExporter : AbstractTRModelExporter<TR3Type, TR3Level, TR3ModelDefinition>
{
    public TR3ModelExporter()
    {
        Data = new TR3DefaultDataProvider();
    }

    protected override AbstractTextureExportHandler<TR3Type, TR3Level, TR3ModelDefinition> CreateTextureHandler()
    {
        return new TR3TextureExportHandler();
    }

    protected override TR3ModelDefinition CreateModelDefinition(TR3Level level, TR3Type modelEntity)
    {
        TR3ModelDefinition definition = new()
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
}
