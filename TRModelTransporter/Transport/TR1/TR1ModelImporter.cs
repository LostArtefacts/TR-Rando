using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Transport;

public class TR1ModelImporter : AbstractTRModelImporter<TR1Type, TR1Level, TR1ModelDefinition>
{
    public TRPalette8Control PaletteManager { get; set; }

    public TR1ModelImporter(bool isCommunityPatch = false)
    {
        Data = new TR1DefaultDataProvider();
        SortModels = true;
        PaletteManager = new();

        if (isCommunityPatch)
        {
            Data.TextureTileLimit = 128;
            Data.TextureObjectLimit = 8192;
        }
    }

    protected override AbstractTextureImportHandler<TR1Type, TR1Level, TR1ModelDefinition> CreateTextureHandler()
    {
        return new TR1TextureImportHandler();
    }

    protected override List<TR1Type> GetExistingModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void Import(IEnumerable<TR1ModelDefinition> standardDefinitions, IEnumerable<TR1ModelDefinition> soundOnlyDefinitions)
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

        foreach (TR1ModelDefinition definition in standardDefinitions)
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
            _textureHandler.ResetUnusedTextures();
            PaletteManager.Dispose();
        }
    }
}
