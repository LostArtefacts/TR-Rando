using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR3DataExporter : TRDataExporter<TR3Type, TR3Level, TR3Blob>
{
    public TR3DataExporter()
    {
        Data = new TR3DataProvider();
    }

    protected override AbstractTextureExportHandler<TR3Type, TR3Level, TR3Blob> CreateTextureHandler()
    {
        return new TR3TextureExportHandler();
    }

    protected override TR3Blob CreateModelDefinition(TR3Level level, TR3Type modelEntity)
    {
        TR3Blob definition = new()
        {
            Alias = modelEntity
        };

        if (Data.IsAlias(modelEntity))
        {
            modelEntity = Data.TranslateAlias(modelEntity);
        }

        ModelTransportHandler.Export(level, definition, modelEntity);
        ColourTransportHandler.Export(level, definition);
        _textureHandler.Export(level, definition, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
        CinematicTransportHandler.Export(level, definition, Data.GetCinematicEntities());
        SoundTransportHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

        return definition;
    }
}
