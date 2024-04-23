using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport;

public class TR3ModelImporter : AbstractTRModelImporter<TR3Type, TR3Level, TR3ModelDefinition>
{
    public TR3ModelImporter()
    {
        Data = new TR3DefaultDataProvider();
        SortModels = true;
    }

    protected override AbstractTextureImportHandler<TR3Type, TR3Level, TR3ModelDefinition> CreateTextureHandler()
    {
        return new TR3TextureImportHandler();
    }

    protected override List<TR3Type> GetExistingModelTypes()
    {
        return Level.Models.Select(m => (TR3Type)m.ID).ToList();
    }

    protected override void Import(IEnumerable<TR3ModelDefinition> standardDefinitions, IEnumerable<TR3ModelDefinition> soundOnlyDefinitions)
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

        foreach (TR3ModelDefinition definition in standardDefinitions)
        {
            if (!IgnoreGraphics)
            {
                ColourTransportHandler.Import(Level, definition);
            }
            MeshTransportHandler.Import(Level, definition);
            CinematicTransportHandler.Import(Level, definition, ForceCinematicOverwrite);
            ModelTransportHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants(), Data.GetUnsafeModelReplacements());
        }

        if (!IgnoreGraphics)
        {
            _textureHandler.ResetUnusedTextures();
        }
    }
}
