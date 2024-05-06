using Newtonsoft.Json;
using TRImageControl.Packing;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR2DataImporter : TRDataImporter<TR2Type, TR2Level, TR2Blob>
{
    public TR2DataImporter()
    {
        Data = new TR2DataProvider();
    }

    protected override AbstractTextureImportHandler<TR2Type, TR2Level, TR2Blob> CreateTextureHandler()
    {
        return new TR2TextureImportHandler();
    }

    protected override List<TR2Type> GetExistingModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void Import(IEnumerable<TR2Blob> standardDefinitions, IEnumerable<TR2Blob> soundOnlyDefinitions)
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

        foreach (TR2Blob definition in standardDefinitions)
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
            //_textureHandler.ResetUnusedTextures();
        }
    }
}
