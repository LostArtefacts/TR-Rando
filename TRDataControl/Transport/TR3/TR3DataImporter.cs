using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport;

public class TR3DataImporter : TRDataImporter<TR3Type, TR3Level, TR3Blob>
{
    public TR3DataImporter()
    {
        Data = new TR3DataProvider();
        SortModels = true;
    }

    protected override AbstractTextureImportHandler<TR3Type, TR3Level, TR3Blob> CreateTextureHandler()
    {
        return new TR3TextureImportHandler();
    }

    protected override List<TR3Type> GetExistingModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void Import(IEnumerable<TR3Blob> standardDefinitions, IEnumerable<TR3Blob> soundOnlyDefinitions)
    {
        TR3TextureRemapGroup remap = null;
        if (TextureRemapPath != null)
        {
            remap = JsonConvert.DeserializeObject<TR3TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
        }

        if (!IgnoreGraphics)
        {
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);
        }

        SoundTransportHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

        Dictionary<TR3Type, TR3Type> aliasPriority = Data.AliasPriority ?? new Dictionary<TR3Type, TR3Type>();

        foreach (TR3Blob definition in standardDefinitions)
        {
            if (!IgnoreGraphics)
            {
                ColourTransportHandler.Import(Level, definition);
            }
            CinematicTransportHandler.Import(Level, definition, ForceCinematicOverwrite);
            ModelTransportHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants(), Data.GetUnsafeModelReplacements());
        }

        if (!IgnoreGraphics)
        {
            _textureHandler.ResetUnusedTextures();
        }
    }
}
