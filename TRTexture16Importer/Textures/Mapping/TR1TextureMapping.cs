using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures
{
    public class TR1TextureMapping : AbstractTextureMapping<TREntities, TRLevel>
    {
        public TR1PaletteManager PaletteManager { get; set; }

        protected TR1TextureMapping(TRLevel level)
            : base(level) { }

        public static TR1TextureMapping Get(TRLevel level, string mappingFilePrefix, TR1TextureDatabase database, Dictionary<StaticTextureSource<TREntities>, List<StaticTextureTarget>> predefinedMapping = null, List<TREntities> entitiesToIgnore = null, Dictionary<TREntities, TREntities> entityMap = null)
        {
            string mapFile = Path.Combine(@"Resources\TR1\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
            if (!File.Exists(mapFile))
            {
                return null;
            }

            TR1TextureMapping mapping = new TR1TextureMapping(level);
            LoadMapping(mapping, mapFile, database, predefinedMapping, entitiesToIgnore);
            mapping.EntityMap = entityMap;
            return mapping;
        }

        protected override TRColour[] GetPalette8()
        {
            return _level.Palette;
        }

        protected override TRColour4[] GetPalette16()
        {
            return null;
        }

        protected override int ImportColour(Color colour)
        {
            if (PaletteManager == null)
            {
                PaletteManager = new TR1PaletteManager
                {
                    Level = _level
                };
            }
            return PaletteManager.AddPredefinedColour(colour);
        }

        protected override TRMesh[] GetModelMeshes(TREntities entity)
        {
            return TRMeshUtilities.GetModelMeshes(_level, entity);
        }

        protected override TRSpriteSequence[] GetSpriteSequences()
        {
            return _level.SpriteSequences;
        }

        protected override TRSpriteTexture[] GetSpriteTextures()
        {
            return _level.SpriteTextures;
        }

        protected override Bitmap GetTile(int tileIndex)
        {
            return _level.Images8[tileIndex].ToBitmap(_level.Palette);
        }

        protected override void SetTile(int tileIndex, Bitmap bitmap)
        {
            if (PaletteManager == null)
            {
                PaletteManager = new TR1PaletteManager
                {
                    Level = _level
                };
            }
            PaletteManager.ChangedTiles[tileIndex] = bitmap;
        }

        public override void CommitGraphics()
        {
            if (!_committed)
            {
                foreach (int tile in _tileMap.Keys)
                {
                    SetTile(tile, _tileMap[tile].Bitmap);
                }

                if (PaletteManager != null)
                {
                    PaletteManager.MergeTiles();
                }

                foreach (int tile in _tileMap.Keys)
                {
                    _tileMap[tile].Dispose();
                }

                _committed = true;
            }
        }
    }
}