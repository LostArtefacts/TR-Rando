using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Textures
{
    public class TR3TextureMapping : AbstractTextureMapping<TR3Entities, TR3Level>
    {
        protected TR3TextureMapping(TR3Level level)
            : base(level) { }

        public static TR3TextureMapping Get(TR3Level level, string mappingFilePrefix, TR3TextureDatabase database, Dictionary<StaticTextureSource<TR3Entities>, List<StaticTextureTarget>> predefinedMapping = null, List<TR3Entities> entitiesToIgnore = null, Dictionary<TR3Entities, TR3Entities> entityMap = null)
        {
            string mapFile = Path.Combine(@"Resources\TR3\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
            if (!File.Exists(mapFile))
            {
                return null;
            }

            TR3TextureMapping mapping = new TR3TextureMapping(level);
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
            return _level.Palette16;
        }

        protected override int ImportColour(Color colour)
        {
            return PaletteUtilities.Import(_level, colour);
        }

        protected override TRMesh[] GetModelMeshes(TR3Entities entity)
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
            return _level.Images16[tileIndex].ToBitmap();
        }

        protected override void SetTile(int tileIndex, Bitmap bitmap)
        {
            _level.Images16[tileIndex].Pixels = TextureUtilities.ImportFromBitmap(bitmap);
        }
    }
}