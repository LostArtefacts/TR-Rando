using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;

namespace TRModelTransporter.Transport
{
    public class TR2ModelImporter : AbstractTRModelImporter<TR2Entities, TR2Level, TR2ModelDefinition>
    {
        public TR2ModelImporter()
        {
            Data = new TR2DefaultDataProvider();
        }

        protected override AbstractTextureImportHandler<TR2Entities, TR2Level, TR2ModelDefinition> CreateTextureHandler()
        {
            return new TR2TextureImportHandler();
        }

        protected override List<TR2Entities> GetExistingModelTypes()
        {
            List<TR2Entities> existingEntities = new List<TR2Entities>();
            Level.Models.ToList().ForEach(m => existingEntities.Add((TR2Entities)m.ID));
            return existingEntities;
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
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);

            // Hardcoded sounds are also imported en-masse to ensure the correct SoundMap indices are assigned
            // before any animation sounds are dealt with.
            _soundHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

            // Allow external alias model priorities to be defined
            Dictionary<TR2Entities, TR2Entities> aliasPriority = Data.AliasPriority ?? new Dictionary<TR2Entities, TR2Entities>();

            foreach (TR2ModelDefinition definition in standardDefinitions)
            {
                // Colours next, again to remap Mesh rectangles/triangles to any new palette indices
                _colourHandler.Import(Level, definition);

                // Meshes and trees should now be remapped, so import into the level
                _meshHandler.Import(Level, definition);

                // Animations, AnimCommands, AnimDispatches, Sounds, StateChanges and Frames
                _animationHandler.Import(Level, definition);

                // Cinematic frames
                _cinematicHandler.Import(Level, definition);

                // Add the model, which will have the correct StartingMesh, MeshTree, Frame and Animation offset.
                _modelHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants());
            }

            _textureHandler.ResetUnusedTextures();
        }
    }
}