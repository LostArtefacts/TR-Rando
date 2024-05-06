using Newtonsoft.Json;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR1DataImporter : TRDataImporter<TR1Type, TR1Level, TR1Blob>
{
    public TRPalette8Control PaletteManager { get; set; }

    public TR1DataImporter(bool isCommunityPatch = false)
    {
        Data = new TR1DataProvider();
        SortModels = true;
        PaletteManager = new();

        if (isCommunityPatch)
        {
            Data.TextureTileLimit = 128;
            Data.TextureObjectLimit = 8192;
        }
    }

    protected override AbstractTextureImportHandler<TR1Type, TR1Level, TR1Blob> CreateTextureHandler()
    {
        return new TR1TextureImportHandler();
    }

    protected override List<TR1Type> GetExistingModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void Import(IEnumerable<TR1Blob> standardDefinitions, IEnumerable<TR1Blob> soundOnlyDefinitions)
    {
        TR1TextureRemapGroup remap = null;
        if (TextureRemapPath != null)
        {
            remap = JsonConvert.DeserializeObject<TR1TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
        }

        if (!IgnoreGraphics)
        {
            PaletteManager.Level = Level;
            PaletteManager.ObsoleteModels = EntitiesToRemove.Select(e => Data.TranslateAlias(e)).ToList();

            (_textureHandler as TR1TextureImportHandler).PaletteManager = PaletteManager;
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);
        }

        SoundTransportHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

        Dictionary<TR1Type, TR1Type> aliasPriority = Data.AliasPriority ?? new Dictionary<TR1Type, TR1Type>();

        foreach (TR1Blob definition in standardDefinitions)
        {
            if (!IgnoreGraphics)
            {
                ColourTransportHandler.Import(definition, PaletteManager);
            }
            CinematicTransportHandler.Import(Level, definition, ForceCinematicOverwrite);
            ModelTransportHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants());
        }

        if (!IgnoreGraphics)
        {
            //_textureHandler.ResetUnusedTextures();
        }
    }
}
