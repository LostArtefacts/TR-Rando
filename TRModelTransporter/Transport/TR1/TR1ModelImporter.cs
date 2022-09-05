using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;
using TRModelTransporter.Model.Textures;
using TRTexture16Importer.Helpers;

namespace TRModelTransporter.Transport
{
    public class TR1ModelImporter : AbstractTRModelImporter<TREntities, TRLevel, TR1ModelDefinition>
    {
        public TR1PaletteManager PaletteManager { get; set; }

        public TR1ModelImporter(bool isCommunityPatch = false)
        {
            Data = new TR1DefaultDataProvider();
            SortModels = true;
            PaletteManager = new TR1PaletteManager();

            if (isCommunityPatch)
            {
                Data.TextureTileLimit = 128;
                Data.TextureObjectLimit = 8192;
            }
        }

        protected override AbstractTextureImportHandler<TREntities, TRLevel, TR1ModelDefinition> CreateTextureHandler()
        {
            return new TR1TextureImportHandler();
        }

        protected override List<TREntities> GetExistingModelTypes()
        {
            List<TREntities> existingEntities = new List<TREntities>();
            Level.Models.ToList().ForEach(m => existingEntities.Add((TREntities)m.ID));
            return existingEntities;
        }

        protected override void Import(IEnumerable<TR1ModelDefinition> standardDefinitions, IEnumerable<TR1ModelDefinition> soundOnlyDefinitions)
        {
            TR1TextureRemapGroup remap = null;
            if (TextureRemapPath != null)
            {
                remap = JsonConvert.DeserializeObject<TR1TextureRemapGroup>(File.ReadAllText(TextureRemapPath));
            }

            PaletteManager.Level = Level;
            PaletteManager.ObsoleteModels = EntitiesToRemove.Select(e => Data.TranslateAlias(e)).ToList();

            (_textureHandler as TR1TextureImportHandler).PaletteManager = PaletteManager;
            _textureHandler.Import(Level, standardDefinitions, EntitiesToRemove, remap, ClearUnusedSprites, TexturePositionMonitor);

            _soundHandler.Import(Level, standardDefinitions.Concat(soundOnlyDefinitions));

            Dictionary<TREntities, TREntities> aliasPriority = Data.AliasPriority ?? new Dictionary<TREntities, TREntities>();

            foreach (TR1ModelDefinition definition in standardDefinitions)
            {
                _colourHandler.Import(Level, definition, PaletteManager);
                _meshHandler.Import(Level, definition);
                _animationHandler.Import(Level, definition);
                _cinematicHandler.Import(Level, definition);
                _modelHandler.Import(Level, definition, aliasPriority, Data.GetLaraDependants());
            }

            _textureHandler.ResetUnusedTextures();

            PaletteManager.Dispose();
        }
    }
}