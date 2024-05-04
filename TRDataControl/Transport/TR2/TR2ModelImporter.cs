using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport;

public class TR2ModelImporter : AbstractTRModelImporter<TR2Type, TR2Level, TR2ModelDefinition>
{
    public TR2ModelImporter()
    {
        Data = new TR2DefaultDataProvider();
    }

    protected override AbstractTextureImportHandler<TR2Type, TR2Level, TR2ModelDefinition> CreateTextureHandler()
    {
        return new TR2TextureImportHandler();
    }

    protected override List<TR2Type> GetExistingModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void Import(IEnumerable<TR2ModelDefinition> standardDefinitions, IEnumerable<TR2ModelDefinition> soundOnlyDefinitions)
    {
        // Textures first, which will remap Mesh rectangles/triangles to the new texture indices.
        // This is called using the entire entity list to import so that RectanglePacker packer has
        // the best chance to organise the tiles.
        TR2TextureRemapGroup remap = null;
        if (TextureRemapPath != null)
        {
            remap = JsonConvert.DeserializeObject<TR2TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
        }

        if (!IgnoreGraphics)
        {
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);
        }

        // Hardcoded sounds are also imported en-masse to ensure the correct SoundMap indices are assigned
        // before any animation sounds are dealt with.
        SoundTransportHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

        // Allow external alias model priorities to be defined
        Dictionary<TR2Type, TR2Type> aliasPriority = Data.AliasPriority ?? new Dictionary<TR2Type, TR2Type>();

        foreach (TR2ModelDefinition definition in standardDefinitions)
        {
            if (!IgnoreGraphics)
            {
                // Colours next, again to remap Mesh rectangles/triangles to any new palette indices
                ColourTransportHandler.Import(Level, definition);
            }

            // Cinematic frames
            CinematicTransportHandler.Import(Level, definition, ForceCinematicOverwrite);

            // Add the model, which will have the correct StartingMesh, MeshTree, Frame and Animation offset.
            ModelTransportHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants());
        }

        if (!IgnoreGraphics)
        {
            _textureHandler.ResetUnusedTextures();
        }
    }
}
